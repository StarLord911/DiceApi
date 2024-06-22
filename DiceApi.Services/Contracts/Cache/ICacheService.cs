using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public interface ICacheService
    {
        Task WriteCache(string key, string value, TimeSpan timeSpan = default);

        Task WriteCache<T>(string key, T value, TimeSpan timeSpan = default);

        Task UpdateCache<T>(string key, T value, TimeSpan timeSpan = default);

        Task<string> ReadCache(string key);

        Task<T> ReadCache<T>(string key);


        Task DeleteCache(string key);
    }
}
