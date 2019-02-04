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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    public static partial class CorePipelineExtensions
    {
        /// <summary>
        /// Attaches the command.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="cpipe">The cpipe.</param>
        /// <param name="command">The command.</param>
        /// <param name="startupPriority">The startup priority.</param>
        /// <param name="assign">The assign.</param>
        /// <param name="channelResponse">The channel response.</param>
        /// <returns></returns>
        public static E AttachCommand<E,C>(this E cpipe
            , C command
            , int startupPriority = 100
            , Action<C> assign = null
            , IPipelineChannelOutgoing<IPipeline> channelResponse = null
            )
            where E: IPipelineChannelIncoming<IPipeline>
            where C : ICommand
        {
            cpipe.Pipeline.AddCommand(command, startupPriority, assign, cpipe, channelResponse);

            return cpipe;
        }
        /// <summary>
        /// Attaches the command.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="cpipe">The cpipe.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="startupPriority">The startup priority.</param>
        /// <param name="assign">The assign.</param>
        /// <param name="channelResponse">The channel response.</param>
        /// <returns></returns>
        public static E AttachCommand<E, P, C>(this E cpipe
            , Func<IEnvironmentConfiguration, C> creator
            , int startupPriority = 100
            , Action<C> assign = null
            , IPipelineChannelOutgoing<IPipeline> channelResponse = null
            )
            where E : IPipelineChannelIncoming<IPipeline>
            //where P : IPipeline
            where C : ICommand
        {
            cpipe.Pipeline.AddCommand(creator, startupPriority, assign, cpipe, channelResponse);

            return cpipe;
        }
    }
}
