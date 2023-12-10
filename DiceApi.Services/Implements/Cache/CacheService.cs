using Microsoft.Extensions.Caching.Distributed;
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

        public async Task<string> ReadCache(string key)
        {
            return await _distributedCache.GetStringAsync(key);
        }

        public async Task WriteCache(string key, string value, TimeSpan timeSpan)
        {
            if (timeSpan == default)
            {
                timeSpan = TimeSpan.FromMinutes(60);
            }

            await _distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = timeSpan });
        }
    }
}
