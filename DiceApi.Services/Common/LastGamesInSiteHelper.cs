using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Common
{
    public static class LastGamesInSiteHelper
    {
        public static List<GameApiModel> UpdateLastGames(List<GameApiModel> games, GameApiModel gameApi) 
        {
            if (games.Count > 10)
            {
                games.Insert(0, gameApi);
                games.RemoveAt(games.Count - 1);
            }
            else
            {
                games.Insert(0, gameApi);
            }

            return games;
        }
    }
}
