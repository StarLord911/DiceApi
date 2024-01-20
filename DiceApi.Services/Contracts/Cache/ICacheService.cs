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

        Task<string> ReadCache(string key);

        Task DeleteCache(string key);
    }
}
