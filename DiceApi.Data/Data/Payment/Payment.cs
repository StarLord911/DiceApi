using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Модель платежа
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// ID в базе данных.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id в платежной системе
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Номер заказа во фри кассе.
        /// </summary>
        public long? FkPaymentId { get; set; }

        /// <summary>
        /// Юзер
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public PaymentStatus Status { get; set; }
    }
}
