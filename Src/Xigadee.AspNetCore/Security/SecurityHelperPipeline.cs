﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Xigadee
{
    /// <summary>
    /// This class is used to help set up the security.
    /// </summary>
    public static class SecurityHelperPipeline
    {
        /// <summary>
        /// Adds the default microservice authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="auth">The authentication settings.</param>
        /// <param name="key">This is the optional key used for token validation. If this is not set, the key value from the config will be parsed.</param>
        /// <returns>Returns the service collection.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services
            , ConfigAuthenticationJwt auth, SecurityKey key = null)
        {
            //services
            //    .AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = auth.Name;
            //        options.DefaultAuthenticateScheme = auth.Name;
            //    })
            //    .AddScheme<ApiAuthenticationSchemeOptions, ApiAuthenticationHandler>(
            //        auth.Name, auth.DisplayName, options =>
            //        {
            //            //options.ClientCertificateThumbprintResolver = new ConditionalCertificateThumbprintResolver(
            //            //    apimRequestIndicator,
            //            //    apimMode,
            //            //    new ApimHttpHeaderCertificateThumbprintResolver(),
            //            //    new HttpConnectionCertificateThumbprintResolver());

            //            //options.ClientIpAddressResolver = new ConditionalIpAddressResolver(
            //            //    apimRequestIndicator,
            //            //    apimMode,
            //            //    new ApimHttpHeaderIpAddressResolver(),
            //            //    new HttpConnectionIpAddressResolver());

            //            options.HttpsOnly = auth.HttpsOnly;

            //            options.JwtBearerTokenOptions = BuildJwtBearerTokenOptions(auth, key);
            //        }
            //    )
            //    ;

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = auth.Name;
                    options.DefaultAuthenticateScheme = auth.Name;
                })
                .AddScheme<JwtApiAuthenticationSchemeOptions, JwtApiAuthenticationHandler>(
                    auth.Name, auth.DisplayName, options =>
                    {
                        options.Configuration = auth;
                    }
                )
                ;

            return services;
        }

        private static IEnumerable<JwtBearerTokenOptions> BuildJwtBearerTokenOptions(ConfigAuthenticationJwt auth, SecurityKey key = null)
        {
            if (key == null)
                key = new SymmetricSecurityKey(Convert.FromBase64String(auth.Key));

            var options = new List<JwtBearerTokenOptions>
            {
                new JwtBearerTokenOptions(
                      new CustomClaimsPrincipalUserResolver()
                    , new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = auth.Audience,
                        ValidateIssuer = true,
                        ValidIssuer = auth.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key ,
                        ValidateLifetime = auth.ValidateTokenExpiry.GetValueOrDefault(true),
                        ClockSkew = auth.GetClockSkew()
                    }
                    , auth.LifetimeWarningDays
                    , auth.LifetimeCriticalDays
                )
            };

            return options;
        }
    }

    /// <summary>
    /// This method extracts the SID parameter from the incoming JWT Token.
    /// </summary>
    /// <seealso cref="Xigadee.IClaimsPrincipalUserReferenceResolver" />
    public class CustomClaimsPrincipalUserResolver : IClaimsPrincipalUserReferenceResolver
    {
        /// <summary>
        /// Resolves the sid id from the claims principal .
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <returns>Returns the user id.</returns>
        public async Task<Guid?> Resolve(ClaimsPrincipal claimsPrincipal)
        {
            var sid = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Sid, StringComparison.InvariantCultureIgnoreCase));

            if (Guid.TryParse(sid?.Value, out var userId))
                return userId;

            return null;
        }
    }
}
