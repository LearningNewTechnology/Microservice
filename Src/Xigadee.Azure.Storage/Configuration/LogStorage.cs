﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Xigadee
{
    public static partial class AzureStorageExtensionMethods
    {
        /// <summary>
        /// The reserved keyword.
        /// </summary>
        public const string LogStorage = "LogStorage";

        /// <summary>
        /// The logging Azure storage account name
        /// </summary>
        [ConfigSettingKey(LogStorage)]
        public const string KeyLogStorageAccountName = "LogStorageAccountName";
        /// <summary>
        /// The logging Azure storage account access key
        /// </summary>
        [ConfigSettingKey(LogStorage)]
        public const string KeyLogStorageAccountAccessKey = "LogStorageAccountAccessKey";


        [ConfigSetting(LogStorage)]
        public static string LogStorageAccountName(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyLogStorageAccountName, config.StorageAccountName());

        [ConfigSetting(LogStorage)]
        public static string LogStorageAccountAccessKey(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyLogStorageAccountAccessKey, config.StorageAccountAccessKey());



        [ConfigSetting(LogStorage)]
        public static StorageCredentials LogStorageCredentials(this IEnvironmentConfiguration config)
        {
            if (string.IsNullOrEmpty(config.LogStorageAccountName()) || string.IsNullOrEmpty(config.LogStorageAccountAccessKey()))
                return config.StorageCredentials();

            return new StorageCredentials(config.LogStorageAccountName(), config.LogStorageAccountAccessKey());
        }

        /// <summary>
        /// This extension allows the Azure log storage values to be manually set as override parameters.
        /// </summary>
        /// <param name="pipeline">The incoming pipeline.</param>
        /// <param name="storageAccountName">The log storage account name.</param>
        /// <param name="storageAccountAccessKey">The log storage account key.</param>
        /// <returns>The pass-through of the pipeline.</returns>
        public static P ConfigOverrideSetAzureLogStorage<P>(this P pipeline, string storageAccountName, string storageAccountAccessKey)
            where P : IPipeline
        {
            pipeline.ConfigurationOverrideSet(KeyLogStorageAccountName, storageAccountName);
            pipeline.ConfigurationOverrideSet(KeyLogStorageAccountAccessKey, storageAccountAccessKey);
            return pipeline;
        }

    }
}
