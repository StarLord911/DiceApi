using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Common
{
    public static class FreeKassaActivePaymentMethodsHelper
    {
        public static List<PaymentFreeKassaMethodInformation> GetPaymentMethodsInfo()
        {
            return new List<PaymentFreeKassaMethodInformation>()
            {
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "Sbp",
                    MethodId = 42,
                    MinDeposited = 1000,
                    MaxDeposit = 300000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "VisaRub",
                    MethodId = 4,
                    MinDeposited = 1000,
                    MaxDeposit = 300000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "TRC20",
                    MethodId = 15,
                    MinDeposited = 5,
                    MaxDeposit = 100000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "ERC20",
                    MethodId = 14,
                    MinDeposited = 10,
                    MaxDeposit = 100000
                }
            };
        }
    }
}
