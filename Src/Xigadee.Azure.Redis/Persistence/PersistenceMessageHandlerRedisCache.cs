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
using System.Threading.Tasks;

namespace Xigadee
{
    public class PersistenceMessageHandlerRedisCache<K,E>: PersistenceManagerHandlerJsonBase<K,E, PersistenceStatistics, PersistenceCommandPolicy>
        where K : IEquatable<K>
    {
        #region Constructor

        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="keyDeserializer"></param>
        /// <param name="entityName">The entity name, derived from E if left null.</param>
        /// <param name="versionPolicy">The optional version and locking policy.</param>
        /// <param name="defaultTimeout">The default timeout when making requests.</param>
        /// <param name="redisConnection"></param>
        /// <param name="keyMaker"></param>
        /// <param name="persistenceRetryPolicy"></param>
        /// <param name="resourceProfile"></param>
        /// <param name="referenceMaker"></param>
        /// <param name="keySerializer"></param>
        public PersistenceMessageHandlerRedisCache(string redisConnection
            , Func<E, K> keyMaker
            , Func<string, K> keyDeserializer
            , string entityName = null
            , VersionPolicy<E> versionPolicy = null
            , TimeSpan? defaultTimeout = null
            , PersistenceRetryPolicy persistenceRetryPolicy = null
            , ResourceProfile resourceProfile = null
            , Func<E, IEnumerable<Tuple<string, string>>> referenceMaker = null
            , Func<K, string> keySerializer = null
            )
            : base( entityName: entityName
                  , versionPolicy: versionPolicy
                  , defaultTimeout: defaultTimeout
                  , persistenceRetryPolicy: persistenceRetryPolicy
                  , resourceProfile: resourceProfile
                  , cacheManager: RedisCacheHelper.Default<K,E>(redisConnection)
                  , keyMaker:keyMaker
                  , referenceMaker:referenceMaker
                  , keySerializer: keySerializer
                  , keyDeserializer: keyDeserializer)
        {
        }
        #endregion

        protected async override Task<IResponseHolder<E>> InternalCreate(K key, PersistenceRequestHolder<K, E> holder)
        {          
            if (await mCacheManager.Write(mTransform, holder.Rq.Entity))
                return new PersistenceResponseHolder<E> { IsSuccess = true, StatusCode = 201, Entity = holder.Rq.Entity };

            return new PersistenceResponseHolder<E> { IsSuccess = false, StatusCode = 409 };
        }

        protected async override Task<IResponseHolder<E>> InternalUpdate(K key, PersistenceRequestHolder<K, E> holder)
        {
            if (await mCacheManager.Write(mTransform, holder.Rq.Entity))
                return new PersistenceResponseHolder<E> { IsSuccess = true, StatusCode = 200, Entity = holder.Rq.Entity };

            return new PersistenceResponseHolder<E> { IsSuccess = false, StatusCode = 409 };
        }

        protected async override Task<IResponseHolder<E>> InternalRead(K key, PersistenceRequestHolder<K, E> holder)
        {
            return await mCacheManager.Read(mTransform, key);
        }

        protected async override Task<IResponseHolder<E>> InternalReadByRef(Tuple<string, string> reference, PersistenceRequestHolder<K, E> holder)
        {
            return await mCacheManager.Read(mTransform, reference);
        }

        protected async override Task<IResponseHolder> InternalVersion(K key, PersistenceRequestHolder<K, Tuple<K, string>> holder)
        {
            return await mCacheManager.VersionRead(mTransform, key);
        }

        protected async override Task<IResponseHolder> InternalVersionByRef(Tuple<string, string> reference, PersistenceRequestHolder<K, Tuple<K, string>> holder)
        {
            return await mCacheManager.VersionRead(mTransform, reference);
        }

        protected async override Task<IResponseHolder> InternalDelete(K key, PersistenceRequestHolder<K, Tuple<K, string>> holder)
        {
            if (await mCacheManager.Delete(mTransform, key))
                return new PersistenceResponseHolder<E> { IsSuccess = true, StatusCode = 200 };

            return new PersistenceResponseHolder<E> { IsSuccess = false, StatusCode = 404 };
        }

        protected async override Task<IResponseHolder> InternalDeleteByRef(Tuple<string, string> reference, PersistenceRequestHolder<K, Tuple<K, string>> holder)
        {
            if (await mCacheManager.Delete(mTransform, reference))
                return new PersistenceResponseHolder<E> { IsSuccess = true, StatusCode = 200 };

            return new PersistenceResponseHolder<E> { IsSuccess = false, StatusCode = 404 };
        }
    }
}
