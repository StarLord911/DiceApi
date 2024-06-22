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
        CardRub = 36,
        Sbp = 42,
        SbpApi = 44,
        UMoney = 6,
        Mir = 12
    }
}
