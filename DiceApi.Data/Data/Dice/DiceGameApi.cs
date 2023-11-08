using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Dice
{
    public class DiceGameApi
    {
        /// <summary>
        /// Какой юзер играл
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        public decimal Sum { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Возможный процент выигрыша
        /// </summary>
        public decimal CanWinSum { get; set; }

        
    }
}
