using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    /// <summary>
    /// Информация о юзере его реффералах и заработке
    /// </summary>
    public class UserRefferalInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public int RefferalCount { get; set; }

        public decimal EarnedMoney { get; set; }
    }
}
