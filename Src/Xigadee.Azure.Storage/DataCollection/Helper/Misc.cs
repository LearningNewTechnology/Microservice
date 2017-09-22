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
using Microsoft.WindowsAzure.Storage.Table;

namespace Xigadee
{
    public static partial class AzureBaseHelper
    {
        private static string DatePartition(DateTime? time = null)
        {
            return (time ?? DateTime.UtcNow).ToString("yyyyMMdd");
        }

        private static string FormatId(Guid? id = null)
        {
            return (id ?? Guid.NewGuid()).ToString("N").ToUpperInvariant();
        }

        public static EntityProperty GetEnum<E>(object value)
        {
            try
            {
                if (value == null)
                    return new EntityProperty("");

                return new EntityProperty(Enum.Format(typeof(E), value, "F"));
            }
            catch (Exception ex)
            {
                return new EntityProperty($"Error-{ex.Message}");
            }
        }

        public static bool SupportsTable(this AzureStorageDataCollectorOptions option)
        {
            return (option.Behaviour & AzureStorageBehaviour.Table) > 0;
        }
        public static bool SupportsBlob(this AzureStorageDataCollectorOptions option)
        {
            return (option.Behaviour & AzureStorageBehaviour.Blob) > 0;
        }
        public static bool SupportsQueue(this AzureStorageDataCollectorOptions option)
        {
            return (option.Behaviour & AzureStorageBehaviour.Queue) > 0;
        }
        public static bool SupportsFile(this AzureStorageDataCollectorOptions option)
        {
            return (option.Behaviour & AzureStorageBehaviour.File) > 0;
        }

        /// <summary>
        /// This method provides the default logging level support for the types of logs.
        /// </summary>
        /// <param name="behaviour">The storage type.</param>
        /// <param name="e">The log event.</param>
        /// <returns>Returns true if the message should be logged.</returns>
        public static bool DefaultLogLevelSupport(AzureStorageBehaviour behaviour, EventHolder ev)
        {
            var e = ev.Data as LogEvent;
            if (e == null)
                return false;

            switch (behaviour)
            {
                case AzureStorageBehaviour.Table:
                    return true;
                case AzureStorageBehaviour.Blob:
                    return e.Level == LoggingLevel.Fatal
                        || e.Level == LoggingLevel.Error
                        || e.Level == LoggingLevel.Warning;
                default:
                    return false;
            }
        }
    }
}
