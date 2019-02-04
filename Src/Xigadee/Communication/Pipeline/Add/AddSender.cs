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
        public static P AddSender<P>(this P pipeline, ISender sender)
            where P : IPipeline
        {
            pipeline.Service.Communication.RegisterSender(sender);

            return pipeline;
        }


        public static P AddSender<P,S>(this P pipeline
            , Func<IEnvironmentConfiguration, S> creator = null
            , Action<S> action = null)
            where P : IPipeline
            where S : ISender, new()
        {
            var sender = creator == null ? new S() : creator(pipeline.Configuration);

            action?.Invoke(sender);

            pipeline.AddSender(sender);

            return pipeline;
        }
    }
}
