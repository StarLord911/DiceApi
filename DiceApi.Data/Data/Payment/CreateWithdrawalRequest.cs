using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Запрос на вывод денег.
    /// </summary>
    public class CreateWithdrawalRequest
    {
        /// <summary>
        /// Юзер
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Сумма вывода
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Номер карты
        /// </summary>
        public string CartNumber { get; set; }
    }
}
