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

#region using
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This class is the base class for components that need to records performance statistics.
    /// </summary>
    /// <typeparam name="S">The base class.</typeparam>
    public abstract class StatisticsBase<S>: IStatisticsBase
        where S : StatusBase, new()
    {
        #region Declarations
        private readonly string mName;
        #endregion
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="name">This sets the name in the statistics.</param>
        protected StatisticsBase(string name = null)
        {
            mName = name ?? GetType().Name;
            StatisticsCreate();
        }
        #endregion

        #region StatisticsInternal
        /// <summary>
        /// This is the internal statistics collection. Components should use this when referencing the collection directly.
        /// Use of Statistics will trigger a recursive StatisticsRecalculate calls each time.
        /// </summary>
        protected internal S StatisticsInternal { get; private set; } 
        #endregion

        #region ComponentId
        /// <summary>
        /// This is the componentId used to uniquely reference the component.
        /// </summary>
        public Guid ComponentId { get; } = Guid.NewGuid(); 
        #endregion
        #region FriendlyName
        /// <summary>
        /// This is the name of the command used in logging based on the name parameter passed in the constructor. 
        /// It defaults to GetType().Name, but can be overridden when generics make this garbled.
        /// </summary>
        public virtual string FriendlyName
        {
            get
            {
                return mName;
            }
        }
        #endregion

        #region StatisticsRecalculate(S statistics)
        /// <summary>
        /// This method is used to set any calculated fields before the statistics are retrieved.
        /// </summary>
        protected abstract void StatisticsRecalculate(S statistics);
        #endregion
        #region StatisticsInitialise(S statistics)
        /// <summary>
        /// This method is used to set any calculated fields before the statistics are retrieved.
        /// </summary>
        protected virtual void StatisticsInitialise(S stats)
        {
            stats.Name = FriendlyName;
            stats.ComponentId = ComponentId;
        }
        #endregion

        #region StatisticsCreate()
        /// <summary>
        /// This method is used to set any calculated fields before the statistics are retrieved.
        /// </summary>
        protected virtual void StatisticsCreate()
        {
            StatisticsInternal = new S();

            StatisticsInitialise(StatisticsInternal);
        }
        #endregion

        #region StatisticsGet()
        /// <summary>
        /// This method allows for other classes to access the statistics class without a reference to the specific generic class.
        /// </summary>
        /// <returns>Returns the base statistics collection class</returns>
        public StatusBase StatisticsGet()
        {
            return StatisticsRecalculated;
        }
        #endregion

        #region StatisticsRecalculated
        /// <summary>
        /// This is the public generic statistics object.
        /// Calling this will result in a recursive call to StatisticsRecalculate
        /// and is provided primarily for external calling parties.
        /// Internal code should use StatisticsInternal to access the collection directly without side effects.
        /// </summary>
        public virtual S StatisticsRecalculated
        {
            get
            {
                try
                {
                    StatisticsRecalculate(StatisticsInternal);
                }
                catch (Exception ex)
                {
                    StatisticsInternal.Ex = ex;
                }

                return StatisticsInternal;
            }
        }
        #endregion
    }
}
