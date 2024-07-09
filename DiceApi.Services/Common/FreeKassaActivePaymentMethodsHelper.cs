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
                    MethodName = "UMoney",
                    MethodId = 6,
                    MinDeposited = 500,
                    MaxDeposit = 75000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "Sbp",
                    MethodId = 42,
                    MinDeposited = 500,
                    MaxDeposit = 300000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "VisaRub",
                    MethodId = 4,
                    MinDeposited = 500,
                    MaxDeposit = 300000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "OnlineBank",
                    MethodId = 13,
                    MinDeposited = 500,
                    MaxDeposit = 75000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "MasterCardRub",
                    MethodId = 8,
                    MinDeposited = 500,
                    MaxDeposit = 300000
                },
                new PaymentFreeKassaMethodInformation()
                {
                    MethodName = "CardRub",
                    MethodId = 36,
                    MinDeposited = 500,
                    MaxDeposit = 75000
                }
            };
        }
    }
}
