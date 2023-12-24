using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Класс хранит инфу об активаций промокода.
    /// </summary>
    public class PrimocodeActivation
    {
        public long UserId { get; set; }

        public string Promocode { get; set; }

        public DateTime ActivationDateTime { get; set; }

        public decimal Wager { get; set; }

        public decimal AddedBallance { get; set; }

    }
}
