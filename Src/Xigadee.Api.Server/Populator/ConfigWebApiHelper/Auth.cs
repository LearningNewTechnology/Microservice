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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    public static class ConfigWebApiHelperAuth
    {
        [ConfigSettingKey("WebApiAuth")]
        public const string KeyAuthAllowInsecureHttp = "AuthAllowInsecureHttp";
        [ConfigSettingKey("WebApiAuth")]
        public const string KeyAuthSignature = "AuthSignature";
        [ConfigSettingKey("WebApiAuth")]
        public const string KeyAuthAudiences = "AuthAudiences";
        [ConfigSettingKey("WebApiAuth")]
        public const string KeyAuthIssuer = "AuthIssuer";
        [ConfigSettingKey("WebApiAuth")]
        public const string KeyAuthTokenLifeInMin = "AuthTokenLifeInMin";

        [ConfigSetting("WebApiAuth")]
        public static bool AuthAllowInsecureHttp(this IEnvironmentConfiguration config) => config.PlatformOrConfigCacheBool(KeyAuthAllowInsecureHttp, "true");

        [ConfigSetting("WebApiAuth")]
        public static Sha512SignatureHelper SignatureProvider(this IEnvironmentConfiguration config) => new Sha512SignatureHelper(config.PlatformOrConfigCache(KeyAuthSignature));

        [ConfigSetting("WebApiAuth")]
        public static string AuthAudiences(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyAuthAudiences);

        [ConfigSetting("WebApiAuth")]
        public static string AuthIssuer(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyAuthIssuer);

        [ConfigSetting("WebApiAuth")]
        public static string AuthTokenLifeInMin(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyAuthTokenLifeInMin, "30");

    }
}
