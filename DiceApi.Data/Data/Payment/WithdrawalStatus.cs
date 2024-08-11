using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public enum WithdrawalStatus
    {
        Moderation = 4,
        AdapterHandle = 2,
        Processed = 6,
        UnConfirmed = 3,
        Error = 5
    }
}
