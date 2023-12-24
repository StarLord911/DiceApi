using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public interface IFakeActiveService
    {
        Task StartFakeOnlineActive();

        Task StartFakeGamesActive();

    }
}
