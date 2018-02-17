﻿//#region Copyright
//// Copyright Hitachi Consulting
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////    http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Xigadee
//{
//    /// <summary>
//    /// These extension methods connect the service bus listeners in to the pipeline.
//    /// </summary>
//    public static partial class AzureExtensionMethods
//    {
//        public static C AttachAzureServiceBusQueueListener<C>(this C cpipe
//            , string connectionName = null
//            , string serviceBusConnection = null
//            , bool isDeadLetterListener = false
//            , string mappingChannelId = null
//            , IEnumerable<ListenerPartitionConfig> priorityPartitions = null
//            , IEnumerable<ResourceProfile> resourceProfiles = null
//            , Action<AzureSBQueueListener> onCreate = null)
//            where C: IPipelineChannelIncoming<IPipeline>
//        {

//            var component = new AzureSBQueueListener();
//            Channel channel = cpipe.ToChannel(ChannelDirection.Incoming);

//            component.ConfigureAzureMessaging(
//                  channel.Id
//                , priorityPartitions ?? channel.Partitions.Cast<ListenerPartitionConfig>()
//                , resourceProfiles
//                , connectionName ?? channel.Id
//                , serviceBusConnection ?? cpipe.Pipeline.Configuration.ServiceBusConnection()
//                );

//            component.IsDeadLetterListener = isDeadLetterListener;

//            onCreate?.Invoke(component);

//            cpipe.AttachListener(component, setFromChannelProperties: false);

//            return cpipe;
//        }




//    }
//}
