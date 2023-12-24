using DiceApi.Data.ApiModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Ответ при досрочкой заверщений игры.
    /// </summary>
    public class FinishMinesGameResponce
    {
        public decimal UserBallance { get; set; }

        public bool Succes { get; set; }

        public string Message { get; set; }

        public string Cells { get; set; }

    }
}
