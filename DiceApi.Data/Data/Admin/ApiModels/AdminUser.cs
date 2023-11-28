using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Admin
{
    public class AdminUser
    {

        public long UserId { get; set; }

        public string Name { get; set; }

        public decimal Ballance { get; set; }

        public string Password { get; set; }

        public DateTime RegistrationDate { get; set; }

        public DateTime LastActiveDate { get; set; }


        public string RegistrationIpAddres { get; set; }

        public string AuthIpAddres { get; set; }

        public decimal BallanceAdd { get; set; }

        /// <summary>
        /// сколько вывел
        /// </summary>
        public decimal ExitBallance { get; set; }

        public int RefferalPercent { get; set; }

        /// <summary>
        /// Сколько заработали
        /// </summary>
        public decimal EarnedMoney { get; set; }

        /// <summary>
        /// Баланс пополненный рефералами.
        /// </summary>
        public decimal ReffsAddedBallance { get; set; }

        public decimal ReffsExitBallance { get; set; }

        public int RefferalCount { get; set; }

        public string Role { get; set; }

        public bool Blocked { get; set; }
    }
}
