using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class PaymentFreeKassaMethodInformation
    {
        public string MethodName { get; set; }

        public int MethodId { get; set; }

        public decimal MinDeposited { get; set; }

        public decimal MaxDeposit { get; set; }
    }
}
