using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для обновление данных юзера.
    /// </summary>
    public class UpdateUserRequest
    {
        public long UserId { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public decimal? Ballance { get; set; }

        public int? ReffetalPercent { get; set; }

        public bool? BlockUser { get; set; }

        public string? BlockReason { get; set; }

        public decimal? PaymentForWithdrawal { get; set; }

        public bool? EnableWithdrawal { get; set; }

        public string? Role { get; set; }

    }
}
