using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class GameApiModel
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
        /// Множитель
        /// </summary>
        [JsonProperty("multiplier")]
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Возможный процент выигрыша
        /// </summary>
        [JsonProperty("canWinSum")]
        public decimal CanWinSum { get; set; }

        /// <summary>
        /// Выиграл или проиграл
        /// </summary>
        [JsonProperty("win")]
        public bool Win { get; set; }

        /// <summary>
        /// Дата игры
        /// </summary>
        [JsonProperty("gameDate")]
        public DateTime GameDate { get; set; }

        /// <summary>
        /// Вид игры
        /// </summary>
        [JsonProperty("gameType")]
        public GameType GameType { get; set; }
    }
}
