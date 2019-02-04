﻿namespace Xigadee
{
    /// <summary>
    /// This enumeration specifies the behaviour that a command should follow when a command throws an error during execution.
    /// </summary>
    public enum ProcessRequestExceptionBehaviour
    {
        /// <summary>
        /// Do Nothing, suppress the exception and don't return any messages to the Dispatcher.
        /// </summary>
        DoNothing,
        /// <summary>
        /// The throw the exception to underlying Dispatcher.
        /// </summary>
        ThrowException,
        /// <summary>
        /// The signal the payload as successful and send a 500 error response in the response.
        /// </summary>
        SignalSuccessAndSend500ErrorResponse,
        /// <summary>
        /// The signal the payload as failed so it will be retried by the underlying architecture and suppress the exception.
        /// </summary>
        SignalFailAndDoNothing,
        /// <summary>
        /// The custom behaviour that can be specified by overriding the processing method. This maps to ThrowException currently.
        /// </summary>
        Custom
    }
}
