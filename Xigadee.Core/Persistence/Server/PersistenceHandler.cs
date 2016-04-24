﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This handler holds and tracks commands to the persistence agent.
    /// </summary>
    public class PersistenceHandler:CommandHandler<PersistenceHandlerStatistics>
    {
        private ConcurrentDictionary<int, PersistenceResponseStatisticsHolder> mResponses = new ConcurrentDictionary<int, PersistenceResponseStatisticsHolder>();

        public virtual void Record<KT, ET>(PersistenceRequestHolder<KT, ET> holder)
        {
            var stats = mResponses.GetOrAdd(holder.Rs.ResponseCode, new PersistenceResponseStatisticsHolder(holder.Rs.ResponseCode));
            stats.Record(holder.Extent, holder.Rs);
        }

        protected override void StatisticsRecalculate(PersistenceHandlerStatistics stats)
        {
            base.StatisticsRecalculate(stats);

            stats.Responses = mResponses.Values.ToArray();
        }
    }

    public class PersistenceHandlerStatistics: CommandHandlerStatistics
    {
        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }

        public PersistenceResponseStatisticsHolder[] Responses { get; set; }
    }

    public class PersistenceResponseStatisticsHolder: MessagingStatistics
    {
        private long mRetries = 0;
        private long mHitCount = 0;

        public PersistenceResponseStatisticsHolder(int status)
        {
            Status = status;
        }

        public int Status { get; }

        public long Retries { get { return mRetries; } }

        public void Record<KT, ET>(TimeSpan? extent, PersistenceRepositoryHolder<KT, ET> rs)
        {
            if (extent.HasValue)
            {
                ActiveIncrement();
                ActiveDecrement(extent.Value);
            }
            
            Interlocked.Add(ref mRetries, rs.Retry);
        }
    }
}