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
    /// This harness is used to set the default configuration for the messaging based service.
    /// </summary>
    /// <typeparam name="L">The messaging class type.</typeparam>
    public abstract class MessagingHarness<L> : ServiceHarness<L>
        where L : class, IMessaging, IService
    {
        /// <summary>
        /// This method sets the channel id and boundary logging status.
        /// </summary>
        /// <param name="service"></param>
        protected override void Configure(L service)
        {
            base.Configure(service);
        }


        /// <summary>
        /// Configures the messaging harness.
        /// </summary>
        /// <param name="configuration">The environment configuration.</param>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="boundaryLoggingActive">Sets boundary logging as active.</param>
        public virtual void Configure(IEnvironmentConfiguration configuration, string channelId
            , bool boundaryLoggingActive = true)
        {
            Service.ChannelId = channelId;
            Service.BoundaryLoggingActive = boundaryLoggingActive;
        }
    }
}
