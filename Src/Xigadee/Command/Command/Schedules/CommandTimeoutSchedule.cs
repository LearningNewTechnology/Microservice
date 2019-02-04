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
using System.Threading;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This schedule class is used to record the timeout count statistics.
    /// </summary>
    public class CommandTimeoutSchedule: CommandJobSchedule
    {
        #region Declarations
        private long mTimeouts = 0;
        #endregion        
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="execute">The function to execute.</param>
        /// <param name="timerConfig">The timer configuration.</param>
        /// <param name="name">The name.</param>
        public CommandTimeoutSchedule(Func<Schedule, CancellationToken, Task> execute
            , ScheduleTimerConfig timerConfig
            , string name)
            : base(execute, timerConfig, name)
        {
        }
        #endregion

        #region Timeouts
        /// <summary>
        /// This is the total count of the number of timeouts.
        /// </summary>
        public long Timeouts
        {
            get { return mTimeouts; }
        }
        #endregion
        #region TimeoutIncrement(long timeouts)
        /// <summary>
        /// This method increments the number of timeouts atomically.
        /// </summary>
        /// <param name="timeouts"></param>
        public void TimeoutIncrement(long timeouts)
        {
            Interlocked.Add(ref mTimeouts, timeouts);
        }
        #endregion

        /// <summary>
        /// This is the debug message used for the statistics.
        /// </summary>
        public override string Debug
        {
            get
            {
                return $"CommandTimeout=[{CommandId.ToString("N")}]:'{Name ?? Id.ToString("N").ToUpperInvariant()}' Active={Active} [ShouldExecute={ShouldExecute}] @ {NextExecuteTime} Run={ExecutionCount}";
            }
        }
    }
}
