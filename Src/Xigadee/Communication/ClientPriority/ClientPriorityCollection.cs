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
using System.Threading;
using System.Threading.Tasks;

#endregion
namespace Xigadee
{
    /// <summary>
    /// This class holds the clients and the polling priority chain.
    /// </summary>
    public class ClientPriorityCollection:StatisticsBase<ClientPriorityCollectionStatistics>
    {
        #region Declarations

        private int[] mListenerPollLevels;

        private Dictionary<Guid, ClientPriorityHolder> mListenerClients;

        private Dictionary<int,Guid[]> mListenerPollChain;

        private readonly long mIteration;

        private long mReprioritise;
        private int mReprioritiseTickCount;

        private readonly IListenerClientPollAlgorithm mPriorityAlgorithm;
        #endregion
        #region Constructor
        /// <summary>
        /// This constructor build up the poll collection and sets the correct poll priority.
        /// </summary>
        /// <param name="listeners"></param>
        /// <param name="resourceTracker"></param>
        /// <param name="algorithm"></param>
        /// <param name="iterationId"></param>
        public ClientPriorityCollection(
              List<IListener> listeners
            , IResourceTracker resourceTracker
            , IListenerClientPollAlgorithm algorithm
            , long iterationId
            )
        {
            mPriorityAlgorithm = algorithm;
            mIteration = iterationId;
            Created = DateTime.UtcNow;

            //Get the list of active clients.
            mListenerClients = new Dictionary<Guid, ClientPriorityHolder>();
            foreach (var listener in listeners)
                if (listener.ListenerClients != null)
                    foreach (var client in listener.ListenerClients)
                    {
                        var holder = new ClientPriorityHolder(resourceTracker, client, listener.ListenerMappingChannelId, algorithm);
                        mListenerClients.Add(holder.Id, holder);
                    }

            //Get the supported levels.
            mListenerPollLevels = mListenerClients
                .Select((c) => c.Value.Priority)
                .Distinct()
                .OrderByDescending((i) => i)
                .ToArray();

            mReprioritise = 0;

            //Finally create a poll chain for each individual priority level
            Reprioritise();
        }
        #endregion

        #region StatisticsRecalculate()
        /// <summary>
        /// This method recalculates the client proiority statistics.
        /// </summary>
        protected override void StatisticsRecalculate(ClientPriorityCollectionStatistics stats)
        {
            stats.Name = $"Iteration {mIteration} @ {Created} {(IsClosed ? "Closed" : "")}";

            var data = new List<ClientPriorityHolderStatistics>();
            foreach (int priority in Levels)
                mListenerPollChain[priority].ForIndex((i, g) =>
                {
                    var stat = mListenerClients[g].StatisticsRecalculated;
                    stat.Ordinal = i;
                    data.Add(stat);
                });

            if (Created.HasValue)
                stats.Created = Created.Value;

            stats.Algorithm = mPriorityAlgorithm.Name;
            stats.Clients = data;
        }
        #endregion

        #region Created
        /// <summary>
        /// This is the time that the collection was created. It is used for reporting a debug purposes.
        /// </summary>
        public DateTime? Created { get;}
        #endregion

        #region Reprioritise()
        /// <summary>
        /// This method re-prioritises the client poll chain based on their calculated priority.
        /// </summary>
        public void Reprioritise()
        {
            //Finally create a poll chain for each individual priority level
            var listenerPollChain = new Dictionary<int, Guid[]>();

            foreach (int priority in mListenerPollLevels)
            {
                var guids = mListenerClients
                    .Where((c) => c.Value.IsActive && c.Value.Priority == priority)
                    .OrderByDescending((c) => c.Value.PriorityRecalculate())
                    .Select((c) => c.Key)
                    .ToArray();

                listenerPollChain.Add(priority, guids);
            }

            Interlocked.Exchange(ref mListenerPollChain, listenerPollChain);

            Interlocked.Increment(ref mReprioritise);
            mReprioritiseTickCount = Environment.TickCount;

            mListenerClients.ForEach((c) => c.Value.CapacityReset());
        }
        #endregion

        #region TakeNext(ITaskAvailability availability, ListenerSlotReservations reservations)
        /// <summary>
        /// This method loops through the client for the given priority while there is slot availability.
        /// </summary>
        /// <param name="availability">This is the current slot availability.</param>
        /// <param name="choosePastDue">This boolean property scans through the client collection but only returns client holders that 
        /// have passed the maximum wait for a poll</param>
        /// <returns>Returns a collection of ClientPriorityHolders.</returns>
        public IEnumerable<ClientPriorityHolder> TakeNext(ITaskAvailability availability, bool choosePastDue)
        {
            var pollChain = mListenerPollChain;
            long iteration = mReprioritise;

            foreach (int priority in pollChain.Keys.OrderByDescending((k) => k))
            {
                //This is the ordered chain of client holders for the particular priority.
                Guid[] priorityChain = pollChain[priority];

                int loopCount = -1;
                while (++loopCount < priorityChain.Length)
                {
                    //Check whether the collection has been reogainsed or closed
                    if (IsClosed || mReprioritise != iteration)
                        yield break;

                    Guid id = priorityChain[loopCount];

                    ClientPriorityHolder holder;
                    //Check whether we can get the client. If not, move to the next.
                    if (!mListenerClients.TryGetValue(id, out holder))
                        continue;

                    //If the holder is already polling then move to the next.
                    if (holder.IsReserved)
                        continue;

                    if (choosePastDue && !holder.IsPollPastDue)
                        continue;

                    //Otherwise, if the holder is infrequently returning data then it will start to 
                    //skip poll slots to speed up retrieval for the active slots.
                    if (!choosePastDue && holder.ShouldSkip())
                        continue;

                    //Ok, check the available amount against the slots that have already been reserved.
                    int actualAvailability = availability.ReservationsAvailable(priority);
                    if (actualAvailability <= 0)
                        continue;

                    int taken;
                    if (!holder.Reserve(actualAvailability, out taken))
                        continue;

                    availability.ReservationMake(holder.Id, priority, taken);

                    //OK, process the holder for polling.
                    yield return holder;
                }
            }
        }
        #endregion
        
        #region Close()
        /// <summary>
        /// This method marks the current collection as closed.
        /// </summary>
        public void Close()
        {
            IsClosed = true;
        }
        #endregion
        #region IsClosed
        /// <summary>
        /// This property is set
        /// </summary>
        public bool IsClosed
        {
            get; private set;
        } 
        #endregion

        #region Levels
        /// <summary>
        /// This is the active priority levels for the listener collection.
        /// </summary>
        public int[] Levels
        {
            get
            {
                return mListenerPollLevels;
            }
        }
        #endregion
        #region Count
        /// <summary>
        /// This is the number of active listeners in the poll chain.
        /// </summary>
        public int Count
        {
            get
            {
                return mListenerPollChain.Values.Sum((g) => g.Length);
            }
        }
        #endregion

        #region Logging
        /// <summary>
        /// This method is called when a poll starts.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="EnqueuedTimeUTC">The current UTC queue time.</param>
        public void QueueTimeLog(Guid clientId, DateTime? EnqueuedTimeUTC)
        {
            if (mListenerClients.ContainsKey(clientId))
            {
                mListenerClients[clientId].QueueTimeLog(EnqueuedTimeUTC);
            }
        }

        /// <summary>
        /// This method is called after a poll has completed.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="start">The poll start tick count.</param>
        public void ActiveDecrement(Guid clientId, int start)
        {
            if (mListenerClients.ContainsKey(clientId))
                mListenerClients[clientId].ActiveDecrement(start);
        }

        /// <summary>
        /// This method increments the error count for a particular client.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        public void ErrorIncrement(Guid clientId)
        {
            if (mListenerClients.ContainsKey(clientId))
                mListenerClients[clientId].ErrorIncrement();
        }
        #endregion
    }
}
