﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{
    /// <summary>
    /// Заявка на вывод
    /// </summary>
    public class Withdrawal
    {
        public long UserId { get; set; }

        public decimal Amount { get; set; }

        public string CardNumber { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsActive { get; set; }
    }
}