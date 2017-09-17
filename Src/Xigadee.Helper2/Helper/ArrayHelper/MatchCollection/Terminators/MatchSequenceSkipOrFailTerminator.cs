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
    /// 
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TMatch">The match type.</typeparam>
    public class MatchSequenceSkipOrFailTerminator<TSource, TMatch> : MatchTerminator<TSource, TMatch>
    {
        #region Constructor
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="Terminator">The sequence to match on.</param>
        public MatchSequenceSkipOrFailTerminator(IEnumerable<TMatch> Terminator)
            : base(Terminator, false)
        {

        }
        /// <summary>
        /// This is the extended constructor.
        /// </summary>
        /// <param name="Terminator"></param>
        /// <param name="CanScan"></param>
        /// <param name="Predicate"></param>
        /// <param name="PredicateTerminator"></param>
        public MatchSequenceSkipOrFailTerminator(IEnumerable<TMatch> Terminator
            , bool CanScan
            , Func<TSource, MatchTerminatorResult, MatchTerminatorStatus> Predicate
            , Func<MatchTerminatorResult
            , Queue<TSource>, TSource, long, bool> PredicateTerminator)
            : base(Terminator, CanScan, Predicate, PredicateTerminator)
        {

        }
        #endregion // Constructor

        #region Validate(TSource item, MatchTerminatorResult currentResult)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentResult"></param>
        /// <returns></returns>
        protected override MatchTerminatorStatus Validate(TSource item, MatchTerminatorResult currentResult)
        {
            bool result = item.Equals(CurrentTerminator.Current);

            if (!result)
                return currentResult.Length == 0 ? MatchTerminatorStatus.SuccessNoLength : MatchTerminatorStatus.Fail;

            return CurrentTerminator.MoveNext() ?
                MatchTerminatorStatus.SuccessPartial : MatchTerminatorStatus.Success;
        }
        #endregion  
        #region ValidateTerminator(MatchTerminatorResult result, Queue<TSource> terminator, TSource currentItem, long length)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="terminator"></param>
        /// <param name="currentItem"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override bool ValidateTerminator(MatchTerminatorResult result, Queue<TSource> terminator, TSource currentItem, long length)
        {
            return result.Length > 0;
        }
        #endregion  
    }
}
