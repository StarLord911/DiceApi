using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Data.Roulette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IRouletteService
    {
        Task<string> BetRouletteGame(CreateRouletteBetRequest request);

        Task<List<RouletteGameResult>> GetLastRouletteGameResults();

        Task<RouletteActiveBets> GetRouletteActiveBets();

    }
}
