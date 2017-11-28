﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Xigadee
{
    /// <summary>
    /// These extensions are used to attach a UDP based listener and sender to a channel
    /// </summary>
    public static partial class UdpCommunicationPipelineExtensions
    {
        public static C AttachUdpListener<C>(this C cpipe
            , IPEndPoint ep
            , Action<UdpContext> convert = null
            , Action<UdpChannelListener> action = null
            )
            where C : IPipelineChannelIncoming<IPipeline>
        {
            var listener = new UdpChannelListener(false, ep, convert);

            cpipe.AttachListener(listener, action, true);

            return cpipe;
        }

        public static C AttachMulticastUdpListener<C>(this C cpipe
            , IPEndPoint ep
            , Action<UdpContext> convert = null
            , Action<UdpChannelListener> action = null
            )
            where C : IPipelineChannelIncoming<IPipeline>
        {
            var listener = new UdpChannelListener(true, ep, convert);

            cpipe.AttachListener(listener, action, true);

            return cpipe;
        }

        public static C AttachUdpSender<C>(this C cpipe
            , IPEndPoint ep
            , Action<UdpContext> convert = null
            , Action<UdpChannelSender> action = null
            )
            where C : IPipelineChannelOutgoing<IPipeline>
        {
            var sender = new UdpChannelSender(false, ep, convert);

            cpipe.AttachSender(sender, action, true);

            return cpipe;
        }


        public static C AttachMulticastUdpSender<C>(this C cpipe
            , IPEndPoint ep
            , Action<UdpContext> convert = null
            , Action<UdpChannelSender> action = null
            )
            where C : IPipelineChannelOutgoing<IPipeline>
        {
            var sender = new UdpChannelSender(true, ep, convert);

            cpipe.AttachSender(sender, action, true);

            return cpipe;
        }
    }
}
