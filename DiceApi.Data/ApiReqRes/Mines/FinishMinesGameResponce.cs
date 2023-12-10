using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    public class FinishMinesGameResponce
    {
        public decimal UserBallance { get; set; }
        public bool Succes { get; set; }

        public string Message { get; set; }
    }
}
