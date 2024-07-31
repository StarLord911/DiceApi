using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Promocode
{
    /// <summary>
    /// Модель реферального промокода для реферальной страницы.
    /// </summary>
    public class RefferalPromocode
    {
        public string Promocode { get; set; }

        public int ActivationCount { get; set; }

        public int ActivatedCount { get; set; }
    }
}
