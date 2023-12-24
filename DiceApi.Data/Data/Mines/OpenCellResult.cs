using DiceApi.Data.ApiModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    public class OpenCellResult
    {
        public bool FindMine { get; set; }

        public bool IsCellOpened { get; set; }

        public bool GameOver { get; set; }

        public decimal CanWin { get; set; }

        public string Cells { get; set; }
    }
}
