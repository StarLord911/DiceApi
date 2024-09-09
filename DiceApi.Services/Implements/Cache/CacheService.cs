using DiceApi.Common;
using EasyMemoryCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICaching _cache;

        public CacheService(ICaching cache)
        {
            _cache = cache;
        }

        public Task WriteCache(string key, string value, TimeSpan timeSpan = default)
        {
            return WriteCache<string>(key, value, timeSpan);
        }

        public async Task WriteCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            var saveDays = 365;
            if (timeSpan != default)
            {
                saveDays = timeSpan.Days;
            }

            await _cache.SetValueToCacheAsync(key, value, saveDays, EasyMemoryCache.Configuration.CacheTimeInterval.Days);
        }

        public Task UpdateCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            return WriteCache(key, value, timeSpan);
        }

        public async Task<T> ReadCache<T>(string key)
        {
            var res = await _cache.GetValueFromCacheAsync<T>(key);

            if (res == null)
            {
                return default(T);
            }

            return res;
        }

        public Task DeleteCache(string key)
        {
            _cache.Invalidate(key);
            return Task.CompletedTask;
        }
    }
}
