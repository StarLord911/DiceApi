using DiceApi.Data.Data.Dice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IDiceGamesRepository
    {

        Task Add(DiceGame diceGame);

        Task<List<DiceGame>> GetAll();

        Task<List<DiceGame>> GetByUserId(long userId);

        Task<List<DiceGame>> GetLastGames();
    }
}
