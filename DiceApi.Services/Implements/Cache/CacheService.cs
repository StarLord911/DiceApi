﻿using DiceApi.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task DeleteCache(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<T> ReadCache<T>(string key)
        {
            var message = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrEmpty(message))
            {
                return default;
            }

            return SerializationHelper.Deserialize<T>(message);

        }

        public async Task UpdateCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            await DeleteCache(key);

            await WriteCache<T>(key, value, timeSpan);
        }

        public async Task WriteCache(string key, string value, TimeSpan timeSpan)
        {

            if (timeSpan == default)
            {
                timeSpan = TimeSpan.FromDays(365);
            }

            await _distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = timeSpan });
        }

        public async Task WriteCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            if (timeSpan == default)
            {
                timeSpan = TimeSpan.FromDays(365);
            }

            var cache = SerializationHelper.Serialize(value);

            await _distributedCache.SetStringAsync(key, cache, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = timeSpan });
        }
    }
}
