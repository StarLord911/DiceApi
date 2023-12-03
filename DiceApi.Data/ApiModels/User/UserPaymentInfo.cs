using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data.ApiModels
{
    public class UserPaymentInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TotalPaymentAmount { get; set; }
    }
}
