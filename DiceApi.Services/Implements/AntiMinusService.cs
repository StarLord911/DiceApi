using DiceApi.Data;
using DiceApi.Services.Common;
using DiceApi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements
{
    public class AntiMinusService : IAntiMinusService
    {
        public bool CheckMinesAntiMinus(ActiveMinesGame minesGame, Settings settings)
        {
            if (minesGame.CanWin >= settings.MinesGameWinningSettings.MinesMaxWinSum)
            {
                return false;
            }

            if (minesGame.Chances.ElementAt(minesGame.OpenedCellsCount - 1) > settings.MinesGameWinningSettings.MinesMaxMultyplayer)
            {
                return false;
            }

            if (minesGame.CanWin >= settings.MinesGameWinningSettings.MinesAntiminusBallance)
            {
                return false;
            }
            var rand = new Random().Next(0, 1);
            if (minesGame.OpenedCellsCount >= (settings.MinesGameWinningSettings.MinesMaxSuccesMineOpens + rand))
            {
                return false;
            }

            return true;
        }
    }
}
