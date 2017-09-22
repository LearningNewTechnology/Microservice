﻿#region Copyright
// Copyright Hitachi Consulting
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Xigadee
{
    /// <summary>
    /// Simple resolver that reads settings using KeyVault.
    /// </summary>
    public class ConfigResolverKeyVault : ConfigResolver
    {
        private readonly Uri mSecretBaseUri;
        private readonly ClientCredential mclientCredential;
        private AuthenticationResult mAuthenticationResult;

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="clientCredential">The client credentials</param>
        /// <param name="secretBaseUri">The Uri.</param>
        public ConfigResolverKeyVault(ClientCredential clientCredential, string secretBaseUri)
        {
            if (clientCredential == null)
                throw new ArgumentNullException(nameof(clientCredential));

            if (secretBaseUri == null)
                throw new ArgumentNullException(nameof(secretBaseUri));

            mclientCredential = clientCredential;
            mSecretBaseUri = new Uri(secretBaseUri);
        }


        /// <summary>
        /// Gets or sets the number of retry attempts when trying to retrieve a value token.
        /// </summary>
        public int NumberOfRetries { get; set; } = 5;

        /// <summary>
        /// Use this method to get the value from the specific resolver.
        /// </summary>
        /// <param name="key">The key to resolve</param>
        /// <returns>
        /// Returns true if it can resolve.
        /// </returns>
        public override bool CanResolve(string key)
        {
            return GetValue(key, NumberOfRetries).Result != null;
        }
        /// <summary>
        /// Use this method to get the value from the specific resolver.
        /// </summary>
        /// <param name="key">The key to resolve</param>
        /// <returns>
        /// This is the settings value, null if not set.
        /// </returns>
        public override string Resolve(string key)
        {
            return GetValue(key, NumberOfRetries).Result;
        }
        /// <summary>
        /// Gets the token from KeyVault.
        /// </summary>
        /// <param name="authority">The authority.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="remainingRetries">The remaining retries.</param>
        /// <returns>Returns the value token as a string, as part of an async request</returns>
        /// <exception cref="ConfigKeyVaultException"></exception>
        protected async Task<string> GetToken(string authority, string resource, string scope, int remainingRetries)
        {
            try
            {
                // If we already have a valid unexpired token then just return this
                if (mAuthenticationResult != null && mAuthenticationResult.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(1))
                    return mAuthenticationResult.AccessToken;

                var authContext = new AuthenticationContext(authority);
                mAuthenticationResult = await authContext.AcquireTokenAsync(resource, mclientCredential);
                return mAuthenticationResult?.AccessToken;
            }
            catch (Exception ex)
            {
                mAuthenticationResult = null;
                if (remainingRetries <= 0)
                    throw new ConfigKeyVaultException($"Unable to retrieve access token for {authority}-{resource}-{scope}", ex);

                // Try again after waiting to give transient errors 
                await Task.Delay(TimeSpan.FromSeconds(NumberOfRetries - remainingRetries));
                return await GetToken(authority, resource, scope, --remainingRetries);
            }
        }
        /// <summary>
        /// Gets the value from the KeyVault client.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="remainingRetries">The remaining retries.</param>
        /// <returns>Returns the value token as a string, as part of an async request</returns>
        /// <exception cref="ConfigKeyVaultException"></exception>
        protected async Task<string> GetValue(string key, int remainingRetries)
        {
            Exception exception;
            var uriSafekey = WebUtility.UrlEncode(key);
            if (string.IsNullOrEmpty(uriSafekey))
                return null;

            try
            {
                var kv = new KeyVaultClient((authority, resource, scope) => GetToken(authority, resource, scope, NumberOfRetries));
                var result = await kv.GetSecretAsync(new Uri(mSecretBaseUri, uriSafekey).AbsoluteUri);
                return result?.Value;
            }
            catch (KeyVaultErrorException ex)
            {
                // Can't find the key so just return a null
                if (ex.Response?.StatusCode == HttpStatusCode.NotFound)
                    return null;

                exception = ex;
            }
            catch (ConfigKeyVaultException)
            {
                // Unable to get authentication token - don't bother with retries here
                throw; 
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (remainingRetries <= 0)
                throw new ConfigKeyVaultException($"Unable to retrieve {key} from key vault", exception);

            // Try again after waiting to give transient errors 
            await Task.Delay(TimeSpan.FromSeconds(NumberOfRetries - remainingRetries));

            return await GetValue(key, --remainingRetries);
        }
    }
}
