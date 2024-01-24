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

        public string RegistrationDate { get; set; }

        public string LastActiveDate { get; set; }

        public string LastAuthIp { get; set; }

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

        public decimal Wager { get; set; }

        public bool Blocked { get; set; }
        /// <summary>
        /// Причина блокировки.
        /// </summary>
        public string BlockReason { get; set; }

        /// <summary>
        /// Какой депозит должен сделать пользователь чтобы создать вывод.
        /// </summary>
        public decimal PaymentForWithdrawal { get; set; }

        public decimal BallanceInGame { get; set; }

        public bool EnabledWithrowal { get; set; }
    }
}
