﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.Data.Dice
{
    public class DiceRequest
    {
        public long UserId { get; set; }

        public int Persent { get; set; }

        public long Sum { get; set; }
    }
}
