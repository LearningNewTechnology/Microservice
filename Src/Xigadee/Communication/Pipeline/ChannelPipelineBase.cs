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
    /// <summary>
    /// This class is used to hold the pipeline configuration.
    /// </summary>
    public abstract class ChannelPipelineBase<P>: MicroservicePipelineExtension<P>, IPipelineChannel<P>
        where P:IPipeline
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="channel">The channel.</param>
        public ChannelPipelineBase(P pipeline, Channel channel):base(pipeline)
        {
            Channel = channel;
        }

        /// <summary>
        /// This is the registered channel
        /// </summary>
        public Channel Channel { get; }
    }
}
