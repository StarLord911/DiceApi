using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes
{
    /// <summary>
    /// Моделька для создания игры в маинс.
    /// </summary>
    public class CreateMinesGameRequest
    {
        public long UserId { get; set; }

        public decimal Sum { get; set; }

        public int MinesCount { get; set; }
    }
}
