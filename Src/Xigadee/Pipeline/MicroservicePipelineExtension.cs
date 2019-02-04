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


namespace Xigadee
{
    /// <summary>
    /// This class is used for configuration that extends the traditional pipeline.
    /// </summary>
    public abstract class MicroservicePipelineExtension<P>: IPipelineExtension<P>
        where P: IPipeline
    {
        /// <summary>
        /// This is the default constructor that sets the underlying pipeline.
        /// </summary>
        /// <param name="pipeline">The base pipeline.</param>
        protected MicroservicePipelineExtension(P pipeline)
        {
            Pipeline = pipeline;
        }

        /// <summary>
        /// This is the configuration pipeline.
        /// </summary>
        public P Pipeline { get; }
    }
}
