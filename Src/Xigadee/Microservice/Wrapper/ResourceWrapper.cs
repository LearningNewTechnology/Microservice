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
    internal class ResourceWrapper: WrapperBase, IMicroserviceResourceMonitor
    {
        ResourceContainer mResourceContainer;

        internal ResourceWrapper(ResourceContainer resourceMonitor, Func<ServiceStatus> getStatus) : base(getStatus)
        {
            mResourceContainer = resourceMonitor;
        }

        public IResourceConsumer RegisterConsumer(string name, ResourceProfile profile)
        {
            return mResourceContainer.RegisterConsumer(name, profile);
        }

        public IResourceRequestRateLimiter RegisterRequestRateLimiter(string name, IEnumerable<ResourceProfile> profiles)
        {
            return mResourceContainer.RegisterRequestRateLimiter(name, profiles);
        }

        public IResourceRequestRateLimiter RegisterRequestRateLimiter(string name, params ResourceProfile[] profiles)
        {
            return mResourceContainer.RegisterRequestRateLimiter(name, profiles);
        }

        public ResourceStatus ResourceStatusGet(string name)
        {
            return mResourceContainer.ResourceStatusGet(name);
        }
    }
}
