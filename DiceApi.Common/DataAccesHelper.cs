using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Common
{
    public static class DataAccesHelper
    {
        public static string GetConnectionString()
        {
            return $@"Server=(localdb)\MSSQLLocalDB;Database=diceDB;Trusted_Connection=False;";

            
        }
    }
}
