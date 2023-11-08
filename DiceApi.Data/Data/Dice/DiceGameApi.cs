using Newtonsoft.Json;
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
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        [JsonProperty("sum")]
        public decimal Sum { get; set; }

        /// <summary>
        /// Ставка
        /// </summary>
        [JsonProperty("multiplier")]
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Возможный процент выигрыша
        /// </summary>
        [JsonProperty("canWinSum")]
        public decimal CanWinSum { get; set; }

        
    }
}
