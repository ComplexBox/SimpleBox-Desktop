using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBox.Models;

namespace SimpleBox.Helpers
{
    public static class ConfigHelper
    {
        public static string GetConfigFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SimpleBox");
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static string GetConfigFileName() => Path.Combine(GetConfigFolder(), "config.json");

        public static Config OpenConfig()
        {
            if (!File.Exists(GetConfigFileName())) return new Config();
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(GetConfigFileName()));
        }

        public static void SaveConfig(Config config)
        {
            File.WriteAllText(
                GetConfigFileName(), JsonConvert.SerializeObject(config));
        }
    }
}
