using DiceApi.Data.Data.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes.Game
{
    public interface IGamesRepository
    {
        Task AddGame(GameModel game);

        Task<List<GameModel>> GetGamesByUserId(long userId);
    }
}
