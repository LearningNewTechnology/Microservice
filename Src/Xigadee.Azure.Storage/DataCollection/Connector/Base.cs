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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Xigadee
{
    /// <summary>
    /// This is the base class shared by all connectors.
    /// </summary>
    /// <typeparam name="O">The request options that determines the retry policy.</typeparam>
    /// <typeparam name="S">The serialization type.</typeparam>
    public abstract class AzureStorageConnectorBase<O,S>: IAzureStorageConnectorBase
        where O: Microsoft.WindowsAzure.Storage.IRequestOptions
    {
        /// <summary>
        /// This is the specific EventBase type supported for the connector.
        /// </summary>
        public DataCollectionSupport Support { get; set; }
        /// <summary>
        /// This is the specific storage options.
        /// </summary>
        public AzureStorageDataCollectorOptions Options { get; set; }
        /// <summary>
        /// This is the cloud storage account used for all connectivity.
        /// </summary>
        public CloudStorageAccount StorageAccount { get; set; }
        /// <summary>
        /// This is the Azure storage operation context.
        /// </summary>
        public OperationContext Context { get; set; }
        /// <summary>
        /// This function is used to create the storage id for the entity;
        /// </summary>
        public Func<EventHolder, MicroserviceId, string> MakeId { get; set; }
        /// <summary>
        /// This function serializes the event entity.
        /// </summary>
        public Func<EventHolder, MicroserviceId, S> Serializer { get; set; }
        /// <summary>
        /// This is the default timeout.
        /// </summary>
        public TimeSpan? DefaultTimeout { get; set; } 
        /// <summary>
        /// This is the root id for the storage container.
        /// </summary>
        public string ContainerId { get; set; }
        /// <summary>
        /// This method returns the default request options if set.
        /// </summary>
        public virtual O RequestOptionsDefault { get; set; }
        /// <summary>
        /// This is the binary encryption handler.
        /// </summary>
        public Func<byte[], byte[]> Encryptor { get; set; }
        /// <summary>
        /// This method writes to the incoming event to the underlying storage technology.
        /// </summary>
        /// <param name="e">The event.</param>
        /// <param name="id">The microservice metadata.</param>
        /// <returns>This is an async task.</returns>
        public abstract Task Write(EventHolder e, MicroserviceId id);
        /// <summary>
        /// This method initializes the connector.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// This method is used to check that the specific event should be written to the underlying storage.
        /// </summary>
        /// <param name="e">The event.</param>
        /// <returns>Returns true if the event should be written.</returns>
        public abstract bool ShouldWrite(EventHolder e);

        /// <summary>
        /// The encryption storage policy.
        /// </summary>
        public AzureStorageEncryption EncryptionPolicy { get; set; }
    }
}
