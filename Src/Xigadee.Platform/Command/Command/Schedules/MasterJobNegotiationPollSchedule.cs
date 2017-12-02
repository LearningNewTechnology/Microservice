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
using System.Threading;
using System.Threading.Tasks;

#endregion
namespace Xigadee
{
    /// <summary>
    /// This schedule is used by the master job poller.
    /// </summary>
    /// <seealso cref="Xigadee.Schedule" />
    public class MasterJobNegotiationPollSchedule: Schedule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MasterJobNegotiationPollSchedule"/> class.
        /// </summary>
        /// <param name="execute">The async schedule function.</param>
        /// <param name="name">The masterjob name.</param>
        public MasterJobNegotiationPollSchedule(Func<Schedule, CancellationToken, Task> execute, string name = null) : base(execute, name, isLongRunning:false)
        {
        }

    }
}
