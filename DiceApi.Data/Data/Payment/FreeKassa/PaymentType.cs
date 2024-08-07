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
        UMoney = 6,
        VisaRub = 4,
        OnlineBank = 13,
        Sbp = 42,
        MasterCardRub = 8,
        CardRub = 36,
        UsdtErc20 = 14,
        UsdtTrc20 = 15

    }

    public enum WithdrawalType
    {
        Sbp = 42,
        CardNumber = 0
    }
}
