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
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
#endregion
namespace Xigadee
{
    public abstract partial class CommandBase<S, P, H>
    {
        #region Declarations
        /// <summary>
        /// This is the collection of in-play messages.
        /// </summary>
        protected ConcurrentDictionary<string, OutgoingRequestTracker> mOutgoingRequests;
        /// <summary>
        /// This is the initiator timeout schedule.
        /// </summary>
        protected CommandTimeoutSchedule mScheduleTimeout;
        #endregion
        #region Events
        /// <summary>
        /// This event is fired when an outgoing request is initiated.
        /// </summary>
        public event EventHandler<OutgoingRequestEventArgs> OnOutgoingRequest;
        /// <summary>
        /// This event is fired when an outgoing request times out.
        /// </summary>
        public event EventHandler<OutgoingRequestEventArgs> OnOutgoingRequestTimeout;
        /// <summary>
        /// This event is fired when an outgoing request completes
        /// </summary>
        public event EventHandler<OutgoingRequestEventArgs> OnOutgoingRequestComplete;
        #endregion

        #region *--> OutgoingRequestsTearUp()
        /// <summary>
        /// This method starts the outgoing request support.
        /// </summary>
        protected virtual void OutgoingRequestsTearUp()
        {
            mOutgoingRequests = new ConcurrentDictionary<string, OutgoingRequestTracker>();

            //Set a timer to signal timeout requests
            mScheduleTimeout = new CommandTimeoutSchedule(TimeOutScheduler
                , Policy.OutgoingRequestsTimeoutPollInterval
                , $"{FriendlyName} Command OutgoingRequests Timeout Poll"
                );
            mScheduleTimeout.CommandId = ComponentId;

            Scheduler.Register(mScheduleTimeout);

            //Check whether the ResponseId has been set, and if not then raise an error 
            //as outgoing messages will not work without a return path.
            if (ResponseId == null)
                throw new CommandStartupException($"Command={GetType().Name}: Outgoing requests are enabled, but the ResponseId parameter has not been set");

            //This is the return message handler
            CommandRegister(ResponseId, OutgoingRequestResponseIn);
        }
        #endregion
        #region *--> OutgoingRequestsTearDown()
        /// <summary>
        /// This method stops the outgoing request supports and marks any pending jobs as cancelled.
        /// </summary>
        protected virtual void OutgoingRequestsTearDown()
        {
            //Remove the filter for the message.
            CommandUnregister(ResponseId, false);

            //Unregister the time out schedule.
            if (mScheduleTimeout != null)
            {
                Scheduler.Unregister(mScheduleTimeout);
                mScheduleTimeout = null;
            }

            try
            {
                foreach (var key in mOutgoingRequests.Keys.ToList())
                    OutgoingRequestTimeout(key);

                mOutgoingRequests.Clear();
                mOutgoingRequests = null;
            }
            catch (Exception ex)
            {
                Collector?.LogException("OutgoingRequestsStop error", ex);
            }
        }
        #endregion

        #region Outgoing
        /// <summary>
        /// Gets or sets the outgoing dispatcher.
        /// </summary>
        protected ICommandOutgoing Outgoing { get; set; } 
        #endregion

        #region UseASPNETThreadModel
        /// <summary>
        /// This property should be set when hosted in an ASP.NET container.
        /// </summary>
        public virtual bool UseASPNETThreadModel { get; set; }
        #endregion
        #region ResponseId
        /// <summary>
        /// This is the internal response id.
        /// </summary>
        private MessageFilterWrapper mResponseId = null;

        /// <summary>
        /// This override will receive the incoming messages
        /// </summary>
        protected virtual MessageFilterWrapper ResponseId
        {
            get
            {
                if (ResponseChannelId == null)
                    return null;

                if (mResponseId == null)
                    mResponseId = new MessageFilterWrapper((ResponseChannelId, FriendlyName, ComponentId.ToString("N").ToUpperInvariant())
                        , OriginatorId.ExternalServiceId);

                return mResponseId;
            }
        }
        #endregion
        #region TaskManager
        /// <summary>
        /// This is the link to the Microservice dispatcher.
        /// </summary>
        public virtual Action<ICommand, string, TransmissionPayload> TaskManager
        {
            get;
            set;
        }
        #endregion

        #region ProcessOutgoing<I, RQ, RS> ...
        /// <summary>
        /// This method is used to send requests to the remote command.
        /// </summary>
        /// <typeparam name="I">The contract interface.</typeparam>
        /// <typeparam name="RQ">The request type.</typeparam>
        /// <typeparam name="RS">The response type.</typeparam>
        /// <param name="rq">The request object.</param>
        /// <param name="rqSettings">The request settings. Use this to specifically set the timeout parameters.</param>
        /// <param name="routingOptions">The routing options by default this will try internal and then external.</param>
        /// <param name="processResponse"></param>
        /// <param name="fallbackMaxProcessingTime">This is the fallback max processing time used if the timeout 
        /// is not set in the request settings. 
        /// If this is also null, the max time out will fall back to the policy settings.</param>
        /// <param name="principal">This is the principal that you wish the command to be executed under. 
        /// By default this is taken from the calling thread if not passed.</param>
        /// <returns>Returns the async response wrapper.</returns>
        /// <returns>Returns a response object of the specified type in a response metadata wrapper.</returns>
        protected internal virtual async Task<ResponseWrapper<RS>> ProcessOutgoing<I, RQ, RS>(RQ rq
            , RequestSettings rqSettings = null
            , ProcessOptions? routingOptions = null
            , Func<TaskStatus, TransmissionPayload, bool, ResponseWrapper<RS>> processResponse = null
            , TimeSpan? fallbackMaxProcessingTime = null
            , IPrincipal principal = null)
            where I : IMessageContract
        {
            string channelId, messageType, actionType;

            if (!ServiceMessageHelper.ExtractContractInfo<I>(out channelId, out messageType, out actionType))
                throw new InvalidOperationException("Unable to locate message contract attributes for " + typeof(I));

            return await ProcessOutgoing<RQ, RS>(channelId, messageType, actionType
                , rq
                , rqSettings
                , routingOptions
                , processResponse
                , fallbackMaxProcessingTime
                , principal ?? Thread.CurrentPrincipal
                );
        }
        #endregion
        #region ProcessOutgoing<RQ, RS> ...
        /// <summary>
        /// This method is used to send requests to the remote command.
        /// </summary>
        /// <typeparam name="RQ">The request type.</typeparam>
        /// <typeparam name="RS">The response type.</typeparam>
        /// <param name="channelId">The header routing information.</param>
        /// <param name="messageType">The header routing information.</param>
        /// <param name="actionType">The header routing information.</param>
        /// <param name="rq">The request object.</param>
        /// <param name="rqSettings">The request settings. Use this to specifically set the timeout parameters.</param>
        /// <param name="routingOptions">The routing options by default this will try internal and then external.</param>
        /// <param name="processResponse"></param>
        /// <param name="fallbackMaxProcessingTime">This is the fallback max processing time used if the timeout 
        /// is not set in the request settings. 
        /// If this is also null, the max time out will fall back to the policy settings.</param>
        /// <param name="principal">This is the principal that you wish the command to be executed under. 
        /// By default this is taken from the calling thread if not passed.</param>
        /// <returns>Returns the async response wrapper.</returns>
        protected internal virtual async Task<ResponseWrapper<RS>> ProcessOutgoing<RQ, RS>(
              string channelId, string messageType, string actionType
            , RQ rq
            , RequestSettings rqSettings = null
            , ProcessOptions? routingOptions = null
            , Func<TaskStatus, TransmissionPayload, bool, ResponseWrapper<RS>> processResponse = null
            , TimeSpan? fallbackMaxProcessingTime = null
            , IPrincipal principal = null
            )
        {
            if (!Policy.OutgoingRequestsEnabled)
                throw new OutgoingRequestsNotEnabledException();

            TransmissionPayload payload = null;
            try
            {
                StatisticsInternal.ActiveIncrement();

                payload = TransmissionPayload.Create(Policy.TransmissionPayloadTraceEnabled);
                payload.SecurityPrincipal = TransmissionPayload.ConvertToClaimsPrincipal(principal ?? Thread.CurrentPrincipal);

                // Set the process correlation key to the correlation id, if passed through the rq settings
                if (!string.IsNullOrEmpty(rqSettings?.CorrelationId))
                    payload.Message.ProcessCorrelationKey = rqSettings.CorrelationId;

                bool processAsync = rqSettings?.ProcessAsync ?? false;

                payload.Options = routingOptions ?? ProcessOptions.RouteExternal | ProcessOptions.RouteInternal;

                //Set the destination message
                payload.Message.ChannelId = channelId ?? ChannelId;
                payload.Message.MessageType = messageType;
                payload.Message.ActionType = actionType;
                payload.Message.ChannelPriority = processAsync ? 0 : 1;

                //Set the response path
                payload.Message.ResponseChannelId = ResponseId.Header.ChannelId;
                payload.Message.ResponseMessageType = ResponseId.Header.MessageType;
                payload.Message.ResponseActionType = ResponseId.Header.ActionType;
                payload.Message.ResponseChannelPriority = payload.Message.ChannelPriority;

                //Set the payload
                payload.Message.Holder = ServiceHandlers.PayloadSerialize(rq);

                //Set the processing time
                payload.MaxProcessingTime = rqSettings?.WaitTime ?? fallbackMaxProcessingTime ?? Policy.OutgoingRequestMaxProcessingTimeDefault;

                //Transmit
                return await OutgoingRequestOut(payload, processResponse ?? ProcessOutgoingResponse<RS>, processAsync);
            }
            catch (Exception ex)
            {
                string key = payload?.Id.ToString() ?? string.Empty;
                Collector?.LogException(string.Format("Error transmitting {0}-{1} internally", actionType, key), ex);
                throw;
            }
        }
        #endregion

        #region OutgoingRequestOut<K>...
        /// <summary>
        /// This method sends a message to the underlying dispatcher and tracks its progress.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="payloadRq">The payload to process.</param>
        /// <param name="processResponse"></param>
        /// <param name="processAsync">Specifies whether the process in async in which case it is 
        /// returned immediately without waiting for the payload to be processed.</param>
        /// <returns>The task with a payload of type K</returns>
        protected async Task<K> OutgoingRequestOut<K>(TransmissionPayload payloadRq,
            Func<TaskStatus, TransmissionPayload, bool, K> processResponse,
            bool processAsync = false)
        {
            ValidateServiceStarted();

            if (payloadRq == null)
                throw new ArgumentNullException("payloadRequest has not been set.");
            if (processResponse == null)
                throw new ArgumentNullException("processPayload has not been set.");

            //Create and register the request holder.
            var tracker = new OutgoingRequestTracker(payloadRq, payloadRq.MaxProcessingTime ?? Policy.OutgoingRequestMaxProcessingTimeDefault);

            //Add the outgoing holder to the collection
            if (!mOutgoingRequests.TryAdd(tracker.Id, tracker))
            {
                var errorStr = $"OutgoingRequestTransmit: Duplicate key {tracker.Id}";

                Collector?.LogMessage(LoggingLevel.Error, errorStr, "RqDuplicate");

                throw new OutgoingRequestTransmitException(errorStr);
            }

            //Raise the event.
            FireAndDecorateEventArgs(OnOutgoingRequest, () => new OutgoingRequestEventArgs(tracker));

            //Submit the payload for processing to the task manager
            TaskManager(this, tracker.Id, tracker.Payload);

            //OK, this is a sync process, let's wait until it responds or times out.
            TransmissionPayload payloadRs = null;

            //This has not been marked async so hold the current task until completion.
            if (!processAsync)
                try
                {
                    if (UseASPNETThreadModel)
                        payloadRs = Task.Run(async () => await tracker.Tcs.Task).Result;
                    else
                        payloadRs = await tracker.Tcs.Task;
                }
                catch (Exception) { }

            return processResponse(tracker.Tcs.Task.Status, payloadRs, processAsync);
        }
        #endregion

        #region ProcessOutgoingResponse<KT, ET>(TaskStatus status, TransmissionPayload prs, bool async)
        /// <summary>
        /// This method is the default method used to process the returning message response.
        /// </summary>
        /// <typeparam name="RS">The response type.</typeparam>
        /// <param name="rType"></param>
        /// <param name="payloadRs">The incoming response payload.</param>
        /// <param name="processAsync"></param>
        /// <returns>Returns the response wrapper generic object.</returns>
        protected virtual ResponseWrapper<RS> ProcessOutgoingResponse<RS>(TaskStatus rType, TransmissionPayload payloadRs, bool processAsync)
        {
            StatisticsInternal.ActiveDecrement(payloadRs?.Extent ?? TimeSpan.Zero);

            if (processAsync)
                return new ResponseWrapper<RS>(responseCode: 202, responseMessage: "Accepted");

            try
            {
                payloadRs?.CompleteSet();

                switch (rType)
                {
                    case TaskStatus.RanToCompletion:
                        try
                        {
                            //payload.Message.
                            var response = new ResponseWrapper<RS>(payloadRs);

                            if (payloadRs.Message.Holder.HasObject)
                                response.Response = (RS)payloadRs.Message.Holder.Object;
                            else if (payloadRs.Message.Holder != null)
                                response.Response = ServiceHandlers.PayloadDeserialize<RS>(payloadRs);

                            return response;
                        }
                        catch (Exception ex)
                        {
                            StatisticsInternal.ErrorIncrement();
                            return new ResponseWrapper<RS>(500, ex.Message);
                        }
                    case TaskStatus.Canceled:
                        StatisticsInternal.ErrorIncrement();
                        return new ResponseWrapper<RS>(408, "Time out");
                    case TaskStatus.Faulted:
                        StatisticsInternal.ErrorIncrement();
                        return new ResponseWrapper<RS>((int)PersistenceResponse.GatewayTimeout504, "Response timeout.");
                    default:
                        StatisticsInternal.ErrorIncrement();
                        return new ResponseWrapper<RS>(500, rType.ToString());
                }
            }
            catch (Exception ex)
            {
                Collector?.LogException("Error processing response for task status " + rType, ex);
                throw;
            }
        }
        #endregion

        #region --> OutgoingRequestResponseIn(TransmissionPayload payload, List<TransmissionPayload> responses)
        /// <summary>
        /// This method processes the returning messages.
        /// </summary>
        /// <param name="payload">The incoming payload.</param>
        /// <param name="responses">The responses collection is not currently used.</param>
        protected virtual Task OutgoingRequestResponseIn(TransmissionPayload payload, List<TransmissionPayload> responses)
        {
            try
            {
                string id = payload?.Message?.CorrelationKey?.ToUpperInvariant();

                //Get the original request.
                OutgoingRequestTracker holder;
                if (id == null || !mOutgoingRequests.TryRemove(id, out holder))
                {
                    //If there is not a match key then quit.
                    Collector?.LogMessage(LoggingLevel.Warning, $"OutgoingRequestResponseIn - id {id ?? "is null"} not matched for {payload?.Message?.ProcessCorrelationKey}", "RqRsMismatch");
                    payload.SignalSuccess();
                    return Task.FromResult(0);
                }

                holder.Tcs.SetResult(payload);

                FireAndDecorateEventArgs(OnOutgoingRequestComplete, () => new OutgoingRequestEventArgs(holder));
            }
            catch (Exception ex)
            {
                Collector?.LogException("OutgoingRequestResponseIn unexpected exception", ex);
            }
            finally
            {
                //Signal to the listener to release the message.
                payload?.SignalSuccess();
            }

            return Task.FromResult(0);
        }
        #endregion

        #region --> TimeOutScheduler...
        /// <summary>
        /// This method receives the timeout timer poll calls.
        /// </summary>
        /// <param name="schedule">The timer schedule.</param>
        /// <param name="token">The cancellation token.</param>
        protected virtual Task TimeOutScheduler(Schedule schedule, CancellationToken token)
        {
            if (!(mOutgoingRequests?.IsEmpty ?? true))
            {
                var timeoutSchedule = schedule as CommandTimeoutSchedule;

                var results = mOutgoingRequests.Values
                    .Where(i => i.HasExpired())
                    .Select((k) => k.Id)
                    .ToList();

                if (results.Count > 0)
                {
                    //Increment the record of time out requests.
                    timeoutSchedule?.TimeoutIncrement(results.Count);

                    //Loop through each time out and cancel it.
                    results.ForEach((id) => OutgoingRequestTimeout(id));
                }
            }

            return Task.FromResult(0);
        }
        #endregion
        #region --> TimeoutTaskManager(string originatorKey)
        /// <summary>
        /// This method is called for processes that support direct notification from the Task Manager 
        /// when a process has been cancelled.
        /// </summary>
        /// <param name="originatorKey">The is the originator tracking key.</param>
        public void TimeoutTaskManager(string originatorKey)
        {
            OutgoingRequestTimeout(originatorKey);
        }
        #endregion

        #region OutgoingRequestTimeout(string id)
        /// <summary>
        /// This method processes the timeout request.
        /// </summary>
        /// <param name="id">The process id.</param>
        protected virtual void OutgoingRequestTimeout(string id)
        {
            try
            {
                //Check that the object has not be removed by another process.
                OutgoingRequestTracker holder;
                if (!mOutgoingRequests.TryRemove(id, out holder))
                    return;

                holder.Tcs.SetCanceled();
                //If there is not a match key then quit.
                Collector?.LogMessage(LoggingLevel.Warning, $"OutgoingRequestTimeout - id={id} has timeout for {holder?.Payload?.Message?.ProcessCorrelationKey}", "RqRsTimeout");

                //Raise the reference to the time out
                FireAndDecorateEventArgs(OnOutgoingRequestTimeout, () => new OutgoingRequestEventArgs(holder));
            }
            catch (Exception ex)
            {
                Collector?.LogException($"OutgoingRequestTimeout exception for {id}", ex);
            }
            finally
            {
                Collector?.LogMessage(LoggingLevel.Info, $"{FriendlyName} received timeout notification for {id}");
            }
        } 
        #endregion

        #region Class -> OutgoingRequestTracker
        /// <summary>
        /// This class holds the additional task information during processing.
        /// </summary>
        [DebuggerDisplay("{Debug}")]
        protected class OutgoingRequestTracker: OutgoingRequest
        {
            /// <summary>
            /// This holder class is used to track outgoing requests.
            /// </summary>
            /// <param name="payload">The payload.</param>
            /// <param name="ttl">The time to live.</param>
            /// <param name="start">The process start tick-count.</param>
            public OutgoingRequestTracker(TransmissionPayload payload, TimeSpan ttl, int? start = null):base(payload, ttl, start)
            {
                Tcs = new TaskCompletionSource<TransmissionPayload>();
            }

            /// <summary>
            /// This is the task holder for the command.
            /// </summary>
            public TaskCompletionSource<TransmissionPayload> Tcs { get; set; }
        }
        #endregion

        #region Class -> OutgoingWrapper<CS, CP, CH>
        /// <summary>
        /// This internal class is used to simplify how commands can communicate between one another.
        /// </summary>
        /// <typeparam name="CS">The statistics class type.</typeparam>
        /// <typeparam name="CP">The customer command policy.</typeparam>
        /// <typeparam name="CH">The command handler type.</typeparam>
        /// <seealso cref="Xigadee.ICommand" />
        protected class OutgoingWrapper<CS, CP, CH>: DispatchWrapper, ICommandOutgoing
            where CS : CommandStatistics, new()
            where CP : CommandPolicy, new()
            where CH : class, ICommandHandler, new()
        {
            TimeSpan? mDefaultRequestTimespan;
            Func<ServiceStatus> mStatus;
            CommandBase<CS, CP, CH> mCommand;

            internal OutgoingWrapper(CommandBase<CS, CP, CH> command
                , TimeSpan? defaultRequestTimespan
                , Func<ServiceStatus> status
                , bool traceEnabled)
                :base(command.ServiceHandlers, (p,s) => command.TaskManager(command, s, p), status, traceEnabled)
            {
                mStatus = status;
                mDefaultRequestTimespan = defaultRequestTimespan;
                mCommand = command;
            }

            /// <summary>
            /// This method is used to send requests to a remote command and wait for a response.
            /// </summary>
            /// <typeparam name="I">The contract interface.</typeparam>
            /// <typeparam name="RQ">The request object type.</typeparam>
            /// <typeparam name="RS">The response object type.</typeparam>
            /// <param name="rq">The request object.</param>
            /// <param name="settings">The request settings. Use this to specifically set the timeout parameters, amongst other settings, or to pass meta data to the calling party.</param>
            /// <param name="routing">The routing options by default this will try internal and then external endpoints.</param>
            /// <param name="principal">This is the principal that you wish the command to be executed under. 
            /// By default this is taken from the calling thread if not passed.</param>
            /// <returns>Returns the async response wrapper.</returns>
            public async Task<ResponseWrapper<RS>> Process<I, RQ, RS>(
                  RQ rq
                , RequestSettings settings = null
                , ProcessOptions? routing = default(ProcessOptions?)
                , IPrincipal principal = null
                )
                where I : IMessageContract
            {
                ValidateServiceStarted();

                return await mCommand.ProcessOutgoing<I, RQ, RS>(rq
                    , settings
                    , routing
                    , fallbackMaxProcessingTime: mDefaultRequestTimespan
                    , principal: principal ?? Thread.CurrentPrincipal
                    );
            }
            /// <summary>
            /// This method is used to send requests to a remote command and wait for a response.
            /// </summary>
            /// <typeparam name="RQ">The request object type.</typeparam>
            /// <typeparam name="RS">The response object type.</typeparam>
            /// <param name="channelId"></param>
            /// <param name="messageType"></param>
            /// <param name="actionType"></param>
            /// <param name="rq">The request object.</param>
            /// <param name="settings">The request settings. Use this to specifically set the timeout parameters, amongst other settings, or to pass meta data to the calling party.</param>
            /// <param name="routing">The routing options by default this will try internal and then external endpoints.</param>
            /// <param name="principal">This is the principal that you wish the command to be executed under. 
            /// By default this is taken from the calling thread if not passed.</param>
            /// <returns>Returns the async response wrapper.</returns>
            public async Task<ResponseWrapper<RS>> Process<RQ, RS>(
                  string channelId, string messageType, string actionType
                , RQ rq
                , RequestSettings settings = null
                , ProcessOptions? routing = default(ProcessOptions?)
                , IPrincipal principal = null
                )
            {
                ValidateServiceStarted();

                return await mCommand.ProcessOutgoing<RQ, RS>(
                      channelId, messageType, actionType
                    , rq
                    , settings
                    , routing
                    , fallbackMaxProcessingTime: mDefaultRequestTimespan
                    , principal: principal ?? Thread.CurrentPrincipal);
            }

            /// <summary>
            /// This method is used to send requests to a remote command and wait for a response.
            /// </summary>
            /// <typeparam name="RQ">The request object type.</typeparam>
            /// <typeparam name="RS">The response object type.</typeparam>
            /// <param name="header">The message header object that defines the remote endpoint.</param>
            /// <param name="rq">The request object.</param>
            /// <param name="settings">The request settings. Use this to specifically set the timeout parameters, amongst other settings, or to pass meta data to the calling party.</param>
            /// <param name="routing">The routing options by default this will try internal and then external endpoints.</param>
            /// <param name="principal">This is the principal that you wish the command to be executed under. 
            /// By default this is taken from the calling thread if not passed.</param>
            /// <returns>Returns the async response wrapper.</returns>
            public async Task<ResponseWrapper<RS>> Process<RQ, RS>(
                  ServiceMessageHeader header
                , RQ rq
                , RequestSettings settings = null
                , ProcessOptions? routing = default(ProcessOptions?)
                , IPrincipal principal = null
                )
            {
                ValidateServiceStarted();

                return await mCommand.ProcessOutgoing<RQ, RS>(
                      header.ChannelId, header.MessageType, header.ActionType
                    , rq
                    , settings
                    , routing
                    , fallbackMaxProcessingTime: mDefaultRequestTimespan
                    , principal: principal ?? Thread.CurrentPrincipal);
            }
        } 
        #endregion
    }

    #region Class -> OutgoingRequest
    /// <summary>
    /// This is the core message used for tracking outgoing messages.
    /// </summary>
    [DebuggerDisplay("{Debug}")]
    public class OutgoingRequest
    {
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="payload">The incoming payload.</param>
        /// <param name="ttl">The time to live for the account.</param>
        /// <param name="start">The timestamp for start.</param>
        public OutgoingRequest(TransmissionPayload payload, TimeSpan ttl, int? start = null)
        {
            Id = payload.Message.OriginatorKey.ToUpperInvariant();
            Payload = payload;
            Start = start ?? Environment.TickCount;
            MaxTTL = ttl;

            ResponseMessage = new ServiceMessageHeader(
                  payload.Message.ResponseChannelId
                , payload.Message.ResponseMessageType
                , payload.Message.ResponseActionType);
        }

        /// <summary>
        /// This is the request Id.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// This incoming payload.
        /// </summary>
        public TransmissionPayload Payload { get; }
        /// <summary>
        /// This is the start timestamp.
        /// </summary>
        public int Start { get; }
        /// <summary>
        /// The maximum time to live for the message.
        /// </summary>
        public TimeSpan MaxTTL { get; }
        /// <summary>
        /// The current timespan since the message began.
        /// </summary>
        public TimeSpan Extent { get { return ExtentNow(); } }
        /// <summary>
        /// The extent since the submitted time or the current time if not set.
        /// </summary>
        /// <param name="now">The optional time to calculate the extent from.</param>
        /// <returns>The timespan.</returns>
        private TimeSpan ExtentNow(int? now = null)
        {
            return ConversionHelper.DeltaAsTimeSpan(Start, now ?? Environment.TickCount).Value;
        }
        /// <summary>
        /// Returns a boolean value indicating whether the message has expired.
        /// </summary>
        /// <param name="now">The time to Timestamp to check from.</param>
        /// <returns>Returns true if the message has expired.</returns>
        public bool HasExpired(int? now = null)
        {
            var extent = ExtentNow();
            return extent > MaxTTL;
        }
        /// <summary>
        /// The address to send the response message.
        /// </summary>
        public ServiceMessageHeader ResponseMessage { get; }
        /// <summary>
        /// The debug string.
        /// </summary>
        public string Debug
        {
            get
            {
                return $"{Id} TTL: {(MaxTTL - Extent).ToFriendlyString()} HasExpired: {(HasExpired() ? "Yes" : "No")}";
            }
        }


    } 
    #endregion
}
