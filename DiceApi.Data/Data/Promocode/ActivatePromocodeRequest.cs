using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class ActivatePromocodeRequest
    {
        public string Promocode { get; set; }

        public long UserId { get; set; }
    }

    public class ActivatePromocodeResponce
    {
        public string Message { get; set; }

        public bool Successful { get; set; }
    }
}
