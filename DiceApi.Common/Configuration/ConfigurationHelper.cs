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
        private static readonly string configFilePath;
        private static readonly Dictionary<string, string> configValues;


        static ConfigHelper()
        {
            var appDirectory = $@"C:\Users\UYANAEV\source\repos\DiceApi\DiceApi.Common\Configuration\";
            configFilePath = Path.Combine(appDirectory, "dice.config");
            configValues = new Dictionary<string, string>();
            LoadConfig();
        }

        private static void LoadConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFilePath);
            XmlNode root = doc.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                string key = node.Attributes["key"].Value;
                string value = node.InnerText;

                configValues[key] = value;
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
