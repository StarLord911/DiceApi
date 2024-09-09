using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface ILogRepository
    {
        Task LogInfo(string message);

        Task LogInfo(string message, string level);

        Task LogGame(string message);

        Task LogError(string message);

        Task LogException(string message, Exception exception);
    }
}
