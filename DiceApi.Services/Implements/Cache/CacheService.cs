using DiceApi.Common;
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
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task WriteCache(string key, string value, TimeSpan timeSpan = default)
        {
            return WriteCache<string>(key, value, timeSpan);
        }

        public Task WriteCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (timeSpan == default)
            {
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromDays(10));
            }
            else
            {
                cacheEntryOptions.SetAbsoluteExpiration(timeSpan);
            }

            var cache = SerializationHelper.Serialize(value);

            _cache.Set(key, cache, cacheEntryOptions);

            return Task.CompletedTask;
        }

        public Task UpdateCache<T>(string key, T value, TimeSpan timeSpan = default)
        {
            return WriteCache(key, value, timeSpan);
        }

        public Task<T> ReadCache<T>(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                // 2. Проверка, является ли значение пустым или null
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    // 3. Десериализация строки в объект типа T
                    var res = SerializationHelper.Deserialize<T>(cachedValue);
                    return Task.FromResult(res);
                }
            }

            // 4. Возвращение значения по умолчанию, если кэш пустой или значение не найдено
            return Task.FromResult(default(T));
        }

        public Task DeleteCache(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
    //public class CacheService : ICacheService
    //{
    //    private readonly IDistributedCache _distributedCache;

    //    public CacheService(IDistributedCache distributedCache)
    //    {
    //        _distributedCache = distributedCache;
    //    }

    //    public async Task<T> ReadCache<T>(string key)
    //    {
    //        var message = await _distributedCache.GetStringAsync(key);

    //        if (string.IsNullOrEmpty(message))
    //        {
    //            return default;
    //        }

    //        return SerializationHelper.Deserialize<T>(message);

    //    }

    //    public async Task WriteCache(string key, string value, TimeSpan timeSpan)
    //    {

    //        if (timeSpan == default)
    //        {
    //            timeSpan = TimeSpan.FromDays(365);
    //        }

    //        await _distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = timeSpan });
    //    }

    //    public async Task UpdateCache<T>(string key, T value, TimeSpan timeSpan = default)
    //    {
    //        var deleteTask = DeleteCache(key);
    //        var writeTask = WriteCache(key, value, timeSpan);

    //        await Task.WhenAll(deleteTask, writeTask);
    //    }

    //    public async Task DeleteCache(string key)
    //    {
    //        await _distributedCache.RemoveAsync(key);
    //    }

    //    public async Task WriteCache<T>(string key, T value, TimeSpan timeSpan = default)
    //    {
    //        if (timeSpan == default)
    //        {
    //            timeSpan = TimeSpan.FromDays(365);
    //        }

    //        var cache = SerializationHelper.Serialize(value);

    //        await _distributedCache.SetStringAsync(key, cache, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = timeSpan });
    //    }
    //}
}
