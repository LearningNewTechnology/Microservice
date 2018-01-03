﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xigadee;

namespace PiO
{
    class Program
    {
        static MicroservicePipeline mservice;

        static void Main(string[] args)
        {
            var name = Dns.GetHostName();
            var host = Dns.GetHostEntry(name);
            var listv4 = host.AddressList.Where((ip) => ip.AddressFamily == AddressFamily.InterNetwork).ToList();
            var listv6 = host.AddressList.Where((ip) => ip.AddressFamily == AddressFamily.InterNetworkV6).ToList();

            DebugMemoryDataCollector coll;
            mservice = new MicroservicePipeline("PiO", description: "PiO Server");

            mservice
                .AdjustPolicyTaskManagerForDebug()
                .ConfigurationSetFromConsoleArgs(args)
                .AddDebugMemoryDataCollector(out coll)
                .AddChannelIncoming("lightwave", "LightwaveRF UDP traffic", ListenerPartitionConfig.Init(1))
                    .AttachUdpListener(UdpConfig.UnicastAllIps(9761)
                        , requestAddress: ("message","in")
                        , deserialize: (holder) => holder.SetObject(new LightwaveMessage(holder.Blob, (IPEndPoint)holder.Metadata)))
                    .AttachCommand(async (ctx) => 
                    {
                        //Do nothing
                        await Task.Delay(10);
                    }
                    , ("message", "in"))
                    .Revert()
                .AddChannelOutgoing("status", "Outgoing UDP status", SenderPartitionConfig.Init(1))
                    .AttachUdpSender(UdpConfig.BroadcastAllIps(44723)
                        , serializer: new StatisticsSummaryLogUdpSerializer())
                    .Revert()
                .OnDataCollection
                (
                    (ctx,ev) => 
                    {
                        ctx.Outgoing.Process(("status", null, null), ev.Data, 1, ProcessOptions.RouteExternal);
                    }
                    , DataCollectionSupport.Statistics
                )
                ;

            mservice.StartWithConsole(args: args);
        }
    }
}