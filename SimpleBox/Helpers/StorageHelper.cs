using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBox.Models;

namespace SimpleBox.Helpers
{
    public static class StorageHelper
    {
        private static string GetConfigFileName() => Path.Combine(ConfigHelper.GetConfigFolder(), "data.json");

        public static MallowSource LoadData()
        {
            if (File.Exists(GetConfigFileName()))
                return JsonConvert.DeserializeObject<MallowSource>(
                    File.ReadAllText(GetConfigFileName()));
            MallowSource source = new MallowSource();
            source.SetValuesOnDeserialized(new StreamingContext());
            return source;
        }

        public static void SaveData(MallowSource data) =>
            File.WriteAllText(
                GetConfigFileName(), JsonConvert.SerializeObject(data));
    }
}
