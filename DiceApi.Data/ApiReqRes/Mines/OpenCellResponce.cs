using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Ответ на открытия ячейки.
    /// </summary>
    public class OpenCellResponce
    {
        public OpenCellResult Result { get; set; }

        public bool Succes { get; set; }
        public string Message { get; set; }

    }
}
