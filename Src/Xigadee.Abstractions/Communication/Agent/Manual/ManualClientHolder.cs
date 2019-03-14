﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This client holder is used by the manual test fabric.
    /// </summary>
    public class ManualClientHolder : ClientHolderV2<MessagingServiceStatistics>
    {
        private ConcurrentQueue<TransmissionPayload> mPending = new ConcurrentQueue<TransmissionPayload>();

        /// <summary>
        /// This action is used to "transmit" a message to the event.
        /// </summary>
        public Action<TransmissionPayload> IncomingAction { get; set; }

        #region StopInternal()
        /// <summary>
        /// Stops the client and purges any pending messages.
        /// </summary>
        protected override void StopInternal()
        {
            Purge();
            base.StopInternal();
        }
        #endregion

        #region Purge()
        /// <summary>
        /// Purges any remaining messages when the service shuts down.
        /// </summary>
        public void Purge()
        {
            TransmissionPayload payload = null;

            while (mPending?.TryDequeue(out payload) ?? false)
            {
                payload.TraceWrite("Purged", instance: Name);
                payload.SignalFail();
            }
        }
        #endregion

        #region Inject(TransmissionPayload payload)
        /// <summary>
        /// This method injects a payload to be picked up by the polling algorithm.
        /// </summary>
        /// <param name="payload">The payload to inject.</param>
        public void Inject(TransmissionPayload payload)
        {
            try
            {
                mPending.Enqueue(payload);
                payload.TraceWrite("Enqueued", instance: Name);
            }
            catch (Exception ex)
            {
                payload.TraceWrite($"Failed: {ex.Message}", instance: Name);
            }
        } 
        #endregion

        /// <summary>
        /// This method pulls fabric messages and converts them in to generic payload messages for the Microservice to process.
        /// </summary>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="wait">The maximum wait in milliseconds</param>
        /// <param name="mappingChannel">This is the incoming mapping channel for subscription based client where the subscription maps
        /// to a new incoming channel on the same topic.</param>
        /// <returns>
        /// Returns a list of transmission for processing.
        /// </returns>
        public override Task<List<TransmissionPayload>> MessagesPull(int? count, int? wait, string mappingChannel = null)
        {
            var list = new List<TransmissionPayload>();

            int countDown = count ?? 1;

            TransmissionPayload payload;

            Guid? batchId = null;
            if (BoundaryLoggingActive)
                batchId = Collector?.BoundaryBatchPoll(count ?? -1, mPending.Count, mappingChannel ?? ChannelId, Priority);

            while (countDown > 0 && mPending.TryDequeue(out payload))
            {
                if (mappingChannel != null)
                    payload.Message.ChannelId = mappingChannel;

                //Get the boundary logger to log the metadata.
                if (BoundaryLoggingActive)
                    Collector?.BoundaryLog(ChannelDirection.Incoming, payload, ChannelId, Priority, batchId: batchId);

                list.Add(payload);
                payload.TraceWrite("Messages Pulled", instance: Name);

                countDown--;
            }

            return Task.FromResult(list);
        }

        /// <summary>
        /// This method is used to Transmit the payload. You should override this method to insert your own transmission logic.
        /// </summary>
        /// <param name="payload">The payload to transmit.</param>
        /// <param name="retry">This parameter specifies the number of retries that should be attempted if transmission fails. By default this value is 0.</param>
        /// <returns></returns>
        /// <exception cref="RetryExceededTransmissionException"></exception>
        public override async Task Transmit(TransmissionPayload payload, int retry = 0)
        {
            bool tryAgain = false;
            bool fail = true;
            try
            {
                LastTickCount = Environment.TickCount;

                if (retry > MaxRetries)
                    throw new RetryExceededTransmissionException();

                IncomingAction?.Invoke(payload);
                payload.TraceWrite("Transmitted", instance: Name);

                if (BoundaryLoggingActive)
                    Collector?.BoundaryLog(ChannelDirection.Outgoing, payload, ChannelId, Priority);

                fail = false;
            }
            catch (Exception ex)
            {
                LogException("Unhandled Exception (Transmit)", ex);
                if (BoundaryLoggingActive)
                    Collector?.BoundaryLog(ChannelDirection.Outgoing, payload, ChannelId, Priority, ex);
                payload.TraceWrite($"Transmit Error {ex.Message}", instance: Name);
                throw;
            }
            finally
            {
                if (fail)
                    StatisticsInternal.ExceptionHitIncrement();
            }

            if (tryAgain)
                await Transmit(payload, ++retry);
        }
    }
}
