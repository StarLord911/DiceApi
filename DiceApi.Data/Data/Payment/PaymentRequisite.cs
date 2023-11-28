using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Payments
{
    /// <summary>
    /// Рекезиты карт юзеров
    /// </summary>
    public class PaymentRequisite
    {
        //TODO: написать для всех карт
        public int Id { get; set; }

        public long UserId { get; set; }

        public string FreeKassaNumber { get; set; }
    }
}
