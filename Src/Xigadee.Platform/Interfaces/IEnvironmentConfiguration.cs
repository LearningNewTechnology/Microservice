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
using System.Collections;
using System.Collections.Generic;

namespace Xigadee
{
    /// <summary>
    /// This interface is used by extension method that provide shortcuts for specific key/value pairs
    /// from the environment configuration.
    /// </summary>
    public interface IEnvironmentConfiguration
    {
        ConfigResolverMemory OverrideSettings { get; } 

        ConfigResolver this[int key] { get; set; }

        int? PriorityAppSettings { get; }

        void ResolversClear();

        void ResolverSet(int priority, ConfigResolver resolver);

        IEnumerable<KeyValuePair<int, ConfigResolver>> Resolvers{get;}

        string PlatformOrConfigCache(string key, string defaultValue = null, bool throwExceptionIfNotFound = false);

        bool PlatformOrConfigCacheBool(string key, string defaultValue = null, bool throwExceptionIfNotFound = false);

        int PlatformOrConfigCacheInt(string key, int? defaultValue = default(int?), bool throwExceptionIfNotFound = false);

        void CacheFlush();

        /// <summary>
        /// This property returns true if all the keys can be resolved.
        /// </summary>
        /// <param name="keys">The collection of keys to resolve.</param>
        /// <returns>Returns true if all keys are resolved.</returns>
        bool CanResolve(params string[] keys);
    }
}