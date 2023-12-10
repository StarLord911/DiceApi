using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Запрос для открытия ячейки.
    /// </summary>
    public class OpenCellRequest
    {
        public long UserId { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

    }
}
