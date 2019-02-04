﻿using System;
using System.Collections.Generic;

namespace Xigadee
{
    public partial class TaskManager
    {
        /// <summary>
        /// This is the policy object for the TaskManager, that determines how it operates.
        /// </summary>
        public class Policy: PolicyBase
        {
            /// <summary>
            /// Gets or sets a value indicating whether the TransmissionPayload trace flag should be set to true.
            /// </summary>
            public bool TransmissionPayloadTraceEnabled { get; set; }

            /// <summary>
            /// This is the internal array containing the priority levels.
            /// </summary>
            protected PriorityLevelReservation[] mPriorityLevels = null;

            private object syncLock = new object();

            #region Constructor
            /// <summary>
            /// This constructor sets the default bulkhead configuration.
            /// </summary>
            public Policy()
            {
                //Set the default bulk head level
                BulkheadReserve(0, 0, 0);
                BulkheadReserve(1, 8, 8);
                BulkheadReserve(2, 2, 2);
                BulkheadReserve(3, 1, 2);
            }
            #endregion
            #region BulkheadReserve(int level, int slotCount, int overage =0)
            /// <summary>
            /// This method can be called to set a bulkhead reservation.
            /// </summary>
            /// <param name="level">The bulkhead level.</param>
            /// <param name="slotCount">The number of slots reserved.</param>
            /// <param name="overage">The permitted overage.</param>
            public void BulkheadReserve(int level, int slotCount, int overage = 0)
            {
                if (level < 0)
                    throw new ArgumentOutOfRangeException("level must be a positive integer");

                var res = new PriorityLevelReservation { Level = level, SlotCount = slotCount, Overage = overage };

                lock (syncLock)
                {
                    if (mPriorityLevels != null && level <= (PriorityLevels - 1))
                    {
                        mPriorityLevels[level] = res;
                        return;
                    }

                    var pLevel = new PriorityLevelReservation[level + 1];

                    if (mPriorityLevels != null)
                        Array.Copy(mPriorityLevels, pLevel, mPriorityLevels.Length);

                    pLevel[level] = res;

                    mPriorityLevels = pLevel;
                }
            }
            #endregion

            /// <summary>
            /// This is the number of priority levels supported in the Task Manager.
            /// </summary>
            public int PriorityLevels { get { return (mPriorityLevels?.Length ?? 0); } }

            /// <summary>
            /// This is the list of priority level reservations.
            /// </summary>
            public IEnumerable<PriorityLevelReservation> PriorityLevelReservations
            {
                get
                {
                    return mPriorityLevels;
                }
            }

            /// <summary>
            /// This is the time that a process is marked as killed after it has been marked as cancelled.
            /// </summary>
            public TimeSpan? ProcessKillOverrunGracePeriod { get; set; } = TimeSpan.FromSeconds(15);
            /// <summary>
            /// This is maximum target percentage usage limit.
            /// </summary>
            public int ProcessorTargetLevelPercentage { get; set; }
            /// <summary>
            /// This is the maximum number overload processes permitted.
            /// </summary>
            public int OverloadProcessLimitMax { get; set; }
            /// <summary>
            /// This is the minimum number of overload processors available.
            /// </summary>
            public int OverloadProcessLimitMin { get; set; }
            /// <summary>
            /// This is the maximum time that an overload process task can run.
            /// </summary>
            public int OverloadProcessTimeInMs { get; set; }
            /// <summary>
            /// This is the maximum number of concurrent requests.
            /// </summary>
            public int ConcurrentRequestsMax { get; set; } = Environment.ProcessorCount * 16;
            /// <summary>
            /// This is the minimum number of concurrent requests.
            /// </summary>
            public int ConcurrentRequestsMin { get; set; } = Environment.ProcessorCount * 2;

            /// <summary>
            /// This is the default time that the process loop should pause before another cycle if it is not triggered
            /// by a task submission or completion. The default is 200 ms.
            /// </summary>
            public int LoopPauseTimeInMs { get; set; } = 50;
            /// <summary>
            /// This override specifies that internal jobs do not get added to the internal queue, but get executed directly.
            /// </summary>
            public bool ExecuteInternalDirect { get; set; } = true;
        }
    }

    /// <summary>
    /// This is the reservation settings for the particular priority level.
    /// </summary>
    public class PriorityLevelReservation
    {
        /// <summary>
        /// This is the priority level.
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// This is the slot count.
        /// </summary>
        public int SlotCount { get; set; }
        /// <summary>
        /// This is the overage limit.
        /// </summary>
        public int Overage { get; set; }
    }
}
