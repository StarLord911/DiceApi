using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes.Thimble
{
    public class BetThimblesRequest
    {
        public long UserId { get; set; }

        public decimal BetSum { get; set; }
    }

    public class BetThimblesResponce
    {
        public bool Succes { get; set; }

        public bool Win { get; set; }

        public string Message { get; set; }

    }
}
