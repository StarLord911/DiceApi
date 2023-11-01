using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Dice
{
    /// <summary>
    /// Класс представляет конкретную игру в даис.
    /// </summary>
    public class DiceGame
    {
        /// <summary>
        /// ID игры
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Какой юзер играл
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Возможный процент выигрыша
        /// </summary>
        public double Persent { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        public decimal Sum { get; set; }

        /// <summary>
        /// Победил или проиграл
        /// </summary>
        public bool Win { get; set; }

        /// <summary>
        /// Возможный выигрыш.
        /// </summary>
        public decimal CanWin { get; set; }

        /// <summary>
        /// Время когда была игры
        /// </summary>
        public DateTime GameDateTime { get; set; }
    }
}
