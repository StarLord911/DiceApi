using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class Promocode
    {
        public long Id { get; set; }

        public string PromoCode { get; set; }

        public int ActivationCount { get; set; }

        public long BallanceAdd { get; set; }

        public bool IsActive { get; set; }

        //отыгрыш.
        public int Wagering { get; set; }

        public bool IsRefferalPromocode { get; set; }

        public long? RefferalPromocodeOwnerId { get; set; }

    }
}
