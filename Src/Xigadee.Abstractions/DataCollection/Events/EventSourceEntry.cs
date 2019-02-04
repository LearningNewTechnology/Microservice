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

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This class is used to log and event source entry.
    /// </summary>
    public class EventSourceEntry: EventSourceEntry<object, object>
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public class EventSourceEntry<K, E>: EventSourceEntryBase
    {
        /// <summary>
        /// The optional key maker.
        /// </summary>
        private Func<K, string> mKeyMaker;

        public EventSourceEntry():this(null){}

        public EventSourceEntry(Func<K, string> keyMaker)
        {
            mKeyMaker = keyMaker ?? ((e) => e.ToString());
        }

        /// <summary>
        /// The entity key.
        /// </summary>
        public K EntityKey { get; set; }
        /// <summary>
        /// The entity.
        /// </summary>
        public E Entity { get; set; }

        /// <summary>
        /// A string representation of the key.
        /// </summary>
        public override string Key
        {
            get
            {
                return mKeyMaker(EntityKey);
            }
        }
    }
}
