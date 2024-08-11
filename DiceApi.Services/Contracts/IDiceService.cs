using DiceApi.Data.Data.Dice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IDiceService
    {
        Task<DiceResponce> StartDice(DiceRequest request);

        Task<List<DiceGame>> GetAllDiceGamesByUserId(long userId);

        Task<List<DiceGame>> GetLastGames();
    }
}
