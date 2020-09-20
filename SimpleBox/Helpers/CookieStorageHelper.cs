using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SimpleBox.Helpers
{
    public static class CookieStorageHelper
    {
        #region Current

        public static CookieCollection CurrentCookies { get; set; } = LoadData();

        #endregion

        #region Storage Helper

        private static string GetConfigFileName() => Path.Combine(ConfigHelper.GetConfigFolder(), "cookies.json");

        private static CookieCollection LoadData()
        {
            try
            {
                if (!File.Exists(GetConfigFileName())) throw new Exception();

                return JsonConvert.DeserializeObject<CookieCollection>(File.ReadAllText(GetConfigFileName()));
            }
            catch (Exception)
            {
                return new CookieCollection();
            }
        }

        public static void SaveData()
        {
            try
            {
                File.WriteAllText(GetConfigFileName(), JsonConvert.SerializeObject(CurrentCookies));
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        #endregion
    }
}
