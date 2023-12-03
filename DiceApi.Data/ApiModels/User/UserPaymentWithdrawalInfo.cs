using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    public class UserPaymentWithdrawalInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TotalPaymentWithdrawalAmount { get; set; }
    }
}
