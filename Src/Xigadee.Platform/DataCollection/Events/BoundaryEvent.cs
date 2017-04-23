﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This is the event class for logging message boundary transitions.
    /// </summary>
    [DebuggerDisplay("{Type} ({ChannelId}|{ChannelPriority}) {Direction} [{Id}]")]
    public class BoundaryEvent: BoundaryEventBase
    {      
        public TransmissionPayload Payload { get; set; }

        public Guid? BatchId { get; set; }

        public int Requested { get; set; }

        public int Actual { get; set; }

        public string ChannelId { get; set; }

        public int ChannelPriority { get; set; }
    }
}