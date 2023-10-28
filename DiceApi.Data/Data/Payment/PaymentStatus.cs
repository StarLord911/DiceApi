using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public enum PaymentStatus
    {
        New = 0,
        Payed = 1,
        Error = 8,
        Cancelled = 9
    }
}
