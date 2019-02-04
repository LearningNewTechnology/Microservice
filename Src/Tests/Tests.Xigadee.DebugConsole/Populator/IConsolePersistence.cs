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
using Xigadee;

namespace Test.Xigadee
{
    internal interface IConsolePersistence<K,E>
        where K: IEquatable<K>
    {
        ServiceStatus Status { get; }

        IRepositoryAsync<K, E> Persistence { get;set; }

        /// <summary>
        /// This event can be used to subscribe to status changes.
        /// </summary>
        event EventHandler<StatusChangedEventArgs> StatusChanged;

        string Name { get; }
    }
}