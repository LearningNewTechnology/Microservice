﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Xigadee
{
    /// <summary>
    /// This class builds up the necessary expression tree.
    /// </summary>
    [DebuggerDisplay("{Debug}")]
    public class ODataExpressionTree
    {
        #region Constructor
        /// <summary>
        /// This is the OData expression tree.
        /// </summary>
        /// <param name="tokensWithoutSeperators">The list of tokens without seperators.</param>
        public ODataExpressionTree(List<ODataTokenBase> tokensWithoutSeperators)
        {
            Components = new ComponentHolder(tokensWithoutSeperators);

            Root = new ODataExpressionNodeGroup(Components);

            Components.Tokens.ForEach(c => Root.Write(c));

            //We're done here.
            Root.Compile();

            Components.Compile();
        }
        #endregion


        #region Root
        /// <summary>
        /// This is the root node for the expression tree.
        /// </summary>
        public ODataExpressionNodeGroup Root { get; private set; } 
        #endregion

        #region Solutions
        /// <summary>
        /// This is a list of valid solutions for the logical collection.
        /// </summary>
        public List<int> Solutions => Components.Solutions;
        #endregion
        #region Params
        /// <summary>
        /// The search parameters.
        /// </summary>
        public Dictionary<int, FilterParameter> Params => Components.HolderParams.ToDictionary((h) => h.Key, (h) => h.Value.FilterParameter);
        #endregion

        #region Components
        /// <summary>
        /// This is the of logical components in the collection.
        /// </summary>
        private ComponentHolder Components { get; }
        #endregion

        #region Class -> ComponentHolder
        /// <summary>
        /// This class holds the specific list of components.
        /// </summary>
        public class ComponentHolder
        {
            private int _componentCount = 0;
            private List<int> _solutions = null;

            public ComponentHolder(List<ODataTokenBase> originalTokens)
            {
                Tokens = originalTokens;
            }

            #region OriginalTokens
            /// <summary>
            /// This is the list of token.
            /// </summary>
            public List<ODataTokenBase> Tokens { get; }
            #endregion


            public int NextComponentId() => ++_componentCount;


            /// <summary>
            /// This method is used to calculate the logical solutions for the expression tree.
            /// </summary>
            public void Compile()
            {
                _solutions = CompileSolutions().ToList();
            }

            #region Solutions
            /// <summary>
            /// This is the list of compiled solutions.
            /// </summary>
            public List<int> Solutions => _solutions; 
            #endregion
            #region CompileSolutions()
            /// <summary>
            /// This method will compile the integer solutions for the searches.
            /// </summary>
            /// <returns>Returns a list of valid integers for the bit positions of each of the searches.</returns>
            public IEnumerable<int> CompileSolutions()
            {
                var max = Math.Pow(2, HolderParams.Count);

                for (int i = 0; i < max; i++)
                    if (CalculateSolution(i))
                        yield return i;
            }
            #endregion

            /// <summary>
            /// This method is used to calculate the solutions.
            /// </summary>
            /// <param name="i">The specific solution.</param>
            /// <returns></returns>
            protected bool CalculateSolution(int i)
            {
                return false;
            }

            #region GroupRegister
            public Dictionary<int, ODataExpressionNodeGroup> HolderGroups { get; } = new Dictionary<int, ODataExpressionNodeGroup>();

            public int GroupRegister(ODataExpressionNodeGroup group)
            {
                int pos = HolderGroups.Count;
                HolderGroups.Add(pos, group);
                return pos;
            } 
            #endregion
            #region SetFilterParameterHolder...
            public Dictionary<int, ODataExpressionNodeFilterParameter> HolderParams { get; } = new Dictionary<int, ODataExpressionNodeFilterParameter>();

            public void SetFilterParameterHolder(ODataExpressionNode node)
            {
                var holder = node as ODataExpressionNodeFilterParameter;

                var param = holder.FilterParameter;
                param.Position = HolderParams.Count + 1;
                HolderParams.Add(param.Position, holder);
            }
            #endregion
            #region SetFilterLogicalHolder...
            public Dictionary<int, ODataExpressionNodeFilterLogical> HolderLogical { get; } = new Dictionary<int, ODataExpressionNodeFilterLogical>();

            public void SetFilterLogicalHolder(ODataExpressionNode node)
            {
                var holder = node as ODataExpressionNodeFilterLogical;

                HolderLogical.Add(HolderLogical.Count, holder);
            }
            #endregion
        } 
        #endregion
    }
}
