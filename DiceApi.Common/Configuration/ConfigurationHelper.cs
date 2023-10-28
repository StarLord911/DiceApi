using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DiceApi.Common.Configuration
{
    public static class ConfigHelper
    {
        private static IConfiguration _configuration;
        private static Dictionary<string, string> configValues;

        public static void LoadConfig(IConfiguration configuration)
        {
            configValues = new Dictionary<string, string>();

            _configuration = configuration;

            foreach (var config in _configuration.AsEnumerable())
            {
                configValues[config.Key] = config.Value;
            }
        }

        public static string GetConfigValue(string key)
        {
            if (configValues.ContainsKey(key))
            {
                return configValues[key];
            }

            throw new Exception($"Config key '{key}' not found");
        }
    }
}
