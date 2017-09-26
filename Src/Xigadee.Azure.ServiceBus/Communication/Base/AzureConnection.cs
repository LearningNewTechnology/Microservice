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
#endregion
namespace Xigadee
{
    /// <summary>
    /// This class holds the Azure connection information.
    /// </summary>
    public class AzureServiceBusConnection
    {
        /// <summary>
        /// This is the default connection.
        /// </summary>
        /// <param name="name">The connection name.</param>
        /// <param name="connection">The azure connection string.</param>
        public AzureServiceBusConnection(string name, string connection)
        {
            if (name == null)
                throw new ArgumentNullException("name", "name cannot be empty for an Azure Service Bus Connection");

            if (string.IsNullOrEmpty(connection))
                throw new ArgumentNullException("connection", "connection cannot be null or empty for an Azure Service Bus Connection");

            ConnectionName = name;
            ConnectionString = connection;
        }
        ///// <summary>
        ///// This is the internal namespace manager.
        ///// </summary>
        //private NamespaceManager mNamespaceManager = null;
        /// <summary>
        /// This is the Azure connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// This is the Azure connection name.
        /// </summary>
        public string ConnectionName { get; set; }
        ///// <summary>
        ///// This is the Azure namespace manager.
        ///// </summary>
        //public NamespaceManager NamespaceManager
        //{ 
        //    get 
        //    {
        //        return mNamespaceManager ?? (mNamespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString));
        //    }
        //}
    }
}
