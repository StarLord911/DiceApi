using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    /// <summary>
    /// Данный класс создан для работы с последими играми на саите.
    /// </summary>
    public interface ILastGamesService
    {
        Task AddLastGames(string userName, decimal betSum, decimal canWin, bool win, GameType gameType);

        Task SendNewLastGames(string userName, decimal betSum, decimal canWin, bool win, GameType gameType);

        Task<List<GameApiModel>> GetLastGames();
    }
}
