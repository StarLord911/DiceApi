using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Payment.FreeKassa
{
    /// <summary>
    /// Payment types for free kassa.
    /// </summary>
    public enum PaymentType
    {
        VisaRub = 4,
        Sbp = 42,

        UsdtErc20 = 14,
        UsdtTrc20 = 15

    }

    public enum WithdrawalType
    {
        Sbp = 42,
        CardNumber = 0
    }
}
