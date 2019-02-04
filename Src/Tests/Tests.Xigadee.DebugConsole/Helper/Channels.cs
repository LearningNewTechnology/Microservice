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

namespace Test.Xigadee
{
    /// <summary>
    /// This helper class contains the test channels for the console application.
    /// </summary>
    internal static class Channels
    {
        public static readonly string TestA = "testa";
        public static readonly string TestB = "testb";
        public static readonly string TestC = "testc";

        public static readonly string Interserve = "interserve";
        public static readonly string MasterJob = "masterjob";

        public static readonly string Internal = "internalpersistence";
        public static readonly string InternalCallback = "internalcallback";
    }
}
