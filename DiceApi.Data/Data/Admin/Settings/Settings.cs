using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class Settings
    {
        public decimal DiceAntiminus { get; set; }

        public decimal MinesAntiminus { get; set; }

        public bool PaymentActive { get; set; }

        public bool WithdrawalActive { get; set; }

        public bool TechnicalWorks {get; set;}
    }
}
