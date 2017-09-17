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
using System.Collections.Generic;

namespace Xigadee
{
    /// <summary>
    /// This class is used to pass meta data to the command request object.
    /// </summary>
    public class RequestSettings
    {
        #region WaitTime
        /// <summary>
        /// This is the time the request should wait for a response until signalling a time out.
        /// </summary>
        public TimeSpan? WaitTime { get; set; }
        #endregion

        #region CorrelationId
        /// <summary>
        /// Shortcut to retrieve the correlation id
        /// </summary>
        public string CorrelationId
        {
            get
            {
                return HeadersGet("X-CorrelationId");
            }
            set
            {
                HeadersSet("X-CorrelationId", value);
            }
        }

        #endregion
        #region ProcessAsync
        /// <summary>
        /// This shortcut method is used to inform the server to process the request asynchronously
        /// </summary>
        public bool ProcessAsync
        {
            get
            {
                return PreferGetBool("processasync", defaultValue: false);
            }
            set
            {
                PreferSet("processasync", value ? "true" : "false");
            }
        }
        #endregion

        /// <summary>
        /// http://tools.ietf.org/html/rfc7240
        /// </summary>
        public Dictionary<string, string> Prefer { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// This collection is a link of key-value pairs that can be passed through to the calling party.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        #region Prefer methods        
        /// <summary>
        /// Gets the prefer option as a boolean value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="trueValue">The true value to compare against.</param>
        /// <param name="defaultValue">The default value if the option cannot be found.</param>
        /// <returns>Returns the boolean prefer option.</returns>
        protected bool PreferGetBool(string key, string trueValue = "true", bool defaultValue = true)
        {
            if (Prefer == null || !Prefer.ContainsKey(key))
                return defaultValue;

            return Prefer[key].Equals(trueValue, StringComparison.InvariantCultureIgnoreCase);
        }
        /// <summary>
        /// Gets a prefer option.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value if the prefer option cannot be found.</param>
        /// <returns>The prefer option value.</returns>
        protected string PreferGet(string key, string defaultValue = null)
        {
            if (Prefer == null || !Prefer.ContainsKey(key))
                return defaultValue;

            return Prefer[key];
        }
        /// <summary>
        /// Set a prefer option.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void PreferSet(string key, string value)
        {
            if (Prefer == null)
                Prefer = new Dictionary<string, string>();

            if (!Prefer.ContainsKey(key))
                Prefer.Add(key, value);
            else
                Prefer[key] = value;
        }
        #endregion
        #region Header methods        
        /// <summary>
        /// Gets a header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value if the header cannot be found..</param>
        /// <returns>The header value.</returns>
        protected string HeadersGet(string key, string defaultValue = null)
        {
            if (Headers == null || !Headers.ContainsKey(key))
                return defaultValue;

            return Headers[key];
        }
        /// <summary>
        /// Sets a header
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void HeadersSet(string key, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();

            if (!Headers.ContainsKey(key))
                Headers.Add(key, value);
            else
                Headers[key] = value;
        }
        #endregion

    }
}
