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
        /// 
        /// </summary>
        /// <param name="cpipe"></param>
        /// <param name="listener"></param>
        /// <param name="setFromChannelProperties"></param>
        /// <returns></returns>
        [Obsolete("This should not be used and will be removed shortly.")]
        public static C AttachManualListener<C>(this C cpipe
            , out ManualChannelListener listener
            , bool setFromChannelProperties = true
            )
            where C: IPipelineChannelIncoming<IPipeline>
        {
            listener = new ManualChannelListener();
            listener.Fabric = new ManualFabricBridge();

            return cpipe.AttachListener(listener, setFromChannelProperties:setFromChannelProperties);
        }
    }
}
