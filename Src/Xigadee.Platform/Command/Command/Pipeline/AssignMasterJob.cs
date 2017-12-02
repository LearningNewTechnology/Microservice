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

using System;

namespace Xigadee
{
    public static partial class CorePipelineExtensions
    {
        /// <summary>
        /// Assigns a master job to the broadcast channel.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="cpipe">The pipeline.</param>
        /// <param name="command">The command.</param>
        /// <param name="assign">The assign.</param>
        /// <param name="channelType">Type of the channel.</param>
        /// <param name="autosetPartition2">Automatically sets the priority partition for master job negotiation to 2.</param>
        /// <returns></returns>
        public static E AssignMasterJob<E, C>(this E cpipe
            , C command
            , Action<C> assign = null
            , string channelType = null
            , bool autosetPartition2 = true
            )
            where E : IPipelineChannelBroadcast<IPipeline>
            where C : ICommand
        {
            command.MasterJobNegotiationChannelMessageType = channelType ?? command.GetType().Name.ToLowerInvariant();

            if (autosetPartition2)
                command.MasterJobNegotiationChannelPriority = 2;

            if (cpipe.ChannelListener != null && command.MasterJobNegotiationChannelIdAutoSet)
                command.MasterJobNegotiationChannelIdIncoming = cpipe.ChannelListener.Id;

            if (cpipe.ChannelSender != null && command.MasterJobNegotiationChannelIdAutoSet)
                command.MasterJobNegotiationChannelIdOutgoing = cpipe.ChannelSender.Id;

            return cpipe;
        }
        /// <summary>
        /// Assigns the master job.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="cpipe">The pipeline.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="assign">The assign.</param>
        /// <param name="channelType">Type of the channel.</param>
        /// <returns></returns>
        public static E AssignMasterJob<E, P, C>(this E cpipe
            , Func<IEnvironmentConfiguration, C> creator
            , Action<C> assign = null
            , string channelType = null
            )
            where E : IPipelineChannelBroadcast<IPipeline>
            where C : ICommand
        {
            var command = creator(cpipe.ToConfiguration());

            cpipe.AssignMasterJob(command, assign, channelType);

            return cpipe;
        }
    }
}
