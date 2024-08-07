using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public enum WithdrawalStatus
    {
        New = 1,
        Moderation = 4,
        Confirmed = 2,
        UnConfirmed = 3,
        Error = 5
    }
}
