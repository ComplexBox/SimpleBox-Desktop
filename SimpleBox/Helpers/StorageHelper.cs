using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBox.Models;

namespace SimpleBox.Helpers
{
    public static class StorageHelper
    {
        private static string GetConfigFileName() => Path.Combine(ConfigHelper.GetConfigFolder(), "data.json");

        public static MallowSource LoadData() =>
            !File.Exists(GetConfigFileName())
                ? new MallowSource()
                : JsonConvert.DeserializeObject<MallowSource>(
                    File.ReadAllText(GetConfigFileName()));

        public static void SaveData(MallowSource data) =>
            File.WriteAllText(
                GetConfigFileName(), JsonConvert.SerializeObject(data));
    }
}
