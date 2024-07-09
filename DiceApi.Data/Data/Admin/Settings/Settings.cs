using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class Settings
    {
        public MinesGameWinningSettings MinesGameWinningSettings { get; set; }

        public DiceGameWinningSettings DiceGameWinningSettings { get; set; }

        public bool PaymentActive { get; set; }

        public bool WithdrawalActive { get; set; }

        public bool TechnicalWorks {get; set;}
    }

    /// <summary>
    /// Класс для антиминуса который хранит настроики побед в играх.
    /// </summary>
    public class MinesGameWinningSettings
    {
        /// <summary>
        /// Максимальный множитель балланса в минах
        /// </summary>
        public int MinesMaxMultyplayer { get; set; }

        /// <summary>
        /// Максимальный выигрыш в минах
        /// </summary>
        public decimal MinesMaxWinSum { get; set; }

        /// <summary>
        /// Максимальное колво мин которые юзер может успешно открыть
        /// </summary>
        public int MinesMaxSuccesMineOpens { get; set; }

        /// <summary>
        /// Балланс антиминуса в минах
        /// </summary>
        public decimal MinesAntiminusBallance { get; set; }
    }

    /// <summary>
    /// Класс для антиминуса который хранит настроики побед в играх.
    /// </summary>
    public class DiceGameWinningSettings
    {
        /// <summary>
        /// Отнимаем доп процент в игре даис
        /// </summary>
        public int DiceMinusPercent { get; set; }

        /// <summary>
        /// Балланс антиминуса в даисе
        /// </summary>
        public decimal DiceAntiminusBallance { get; set; }
    }
}
