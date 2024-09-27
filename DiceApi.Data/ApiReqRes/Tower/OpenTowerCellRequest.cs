using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiReqRes.Tower
{
    public class OpenTowerCellRequest
    {
      
            public long UserId { get; set; }

            public int CellId { get; set; }

        
    }

    public class OpenTowerCellResponce
    {
        public OpenCellResult Result { get; set; }

        public bool Succes { get; set; }

        public string Message { get; set; }
    }

    public class FinishTowerGameResponce
    {
        public decimal UserBallance { get; set; }

        public bool Succes { get; set; }

        public string Message { get; set; }

        public string Cells { get; set; }

    }
}
