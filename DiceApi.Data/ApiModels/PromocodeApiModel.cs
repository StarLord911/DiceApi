using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    public class PromocodeApiModel
    {
        public string Name { get; set; }

        public decimal BallanceAdd { get; set; }

        public int Wagering { get; set; }

        public int AllActivationCount { get; set; }

        public int ActivatedCount { get; set; }

    }
}
