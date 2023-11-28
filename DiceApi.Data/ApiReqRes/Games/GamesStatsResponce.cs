using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Класс хранит инфу о статистике игры игрока
    /// </summary>
    public class GamesStatsResponce
    {
        /// <summary>
        /// Сумма выигранная в даис
        /// </summary>
        public decimal DiceWinSum { get; set; }
        
        /// <summary>
        /// Сумма проигранная в даис
        /// </summary>
        public decimal DiceLoseSum { get; set; }

        /// <summary>
        /// Количество игр в даис
        /// </summary>
        public int DiceBetCount { get; set; }

        /// <summary>
        /// Общая сумма ставок в даис
        /// </summary>
        public decimal DiceAllBetsSum { get; set; }

        /// <summary>
        /// Сумма выигранная в маинс
        /// </summary>
        public decimal MinesWinSum { get; set; }

        /// <summary>
        /// Сумма проигранная в маинс
        /// </summary>
        public decimal MinesLoseSum { get; set; }

        /// <summary>
        /// Количество игр в маинс
        /// </summary>
        public int MinesBetCount { get; set; }

        /// <summary>
        /// Общая сумма ставок в маинс
        /// </summary>
        public decimal MinesAllBetsSum { get; set; }


    }
}
