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

        public static ObservableCollection<MallowGroup> LoadData() =>
            !File.Exists(GetConfigFileName())
                ? new ObservableCollection<MallowGroup>()
                : JsonConvert.DeserializeObject<ObservableCollection<MallowGroup>>(
                    File.ReadAllText(GetConfigFileName()));

        public static void SaveData(ObservableCollection<MallowGroup> data) =>
            File.WriteAllText(
                GetConfigFileName(), JsonConvert.SerializeObject(data));
    }
}
