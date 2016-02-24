﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This interface is implemented by the cache service that can be attached to an existing persistence agent
    /// to provide fast cache support when needed.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public interface ICacheManager<K,E>
        where K : IEquatable<K>
    {
        bool IsReadOnly { get; }

        bool IsActive { get; }

        //Task<bool> CanRead(K key);

        //Task<Tuple<bool, E>> Read(K key);

        //Task<bool> Write(K key, E Entity, TimeSpan? expiry = null);

        //Task<bool> Clear(K key);
    }
}
