using DiceApi.Data.ApiReqRes;
using DiceApi.Data.ApiReqRes.HorseRace;
using DiceApi.Data.Data.HorseGame;
using DiceApi.Data.Data.Roulette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IHorseRaceService
    {
        public Task<CreateHorseBetResponce> BetHorceRace(CreateHorseBetRequest request);

        Task<List<HorseGameResult>> GetLastHorseGameResults();
        
        Task<HorseRaceActiveBets> GetHorseGameActiveBets();
    }
}
