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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
#endregion
namespace Xigadee
{
    public class ActionQueueCollectionStatistics: MessagingStatistics, ICollectionStatistics
    {
        public int ItemCount { get; set; }

        public int QueueLength { get; set; }

        public bool Overloaded { get; set; }

        public int OverloadProcessCount { get; set; }

        public int? OverloadThreshold { get; set; }

        public long OverloadProcessHits { get; set; }


        public List<object> Components { get; set; }

    }

    public class ActionQueueStatistics: MessagingStatistics
    {

    }
}
