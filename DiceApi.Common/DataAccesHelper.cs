using DiceApi.Common.Configuration;
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
            return ConfigHelper.GetConfigValue(ConfigerationNames.DbConnectionString);
        }
    }
}
