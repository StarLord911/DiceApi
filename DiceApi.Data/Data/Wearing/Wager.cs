using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class Wager
    {
        /// <summary>
        /// Идентификатор промокода
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Пользователь который должен отыграть
        /// </summary>
        public long UserId { get; set; }
        
        /// <summary>
        /// Сколько нужно отыграть по промокоду
        /// </summary>
        public decimal Wagering { get; set; }

        /// <summary>
        /// Сколько отыграно по промокоду
        /// </summary>
        public decimal Played { get; set; }

        /// <summary>
        /// Активен ли отыгрыш
        /// </summary>
        public bool IsActive { get; set; }


    }
}
