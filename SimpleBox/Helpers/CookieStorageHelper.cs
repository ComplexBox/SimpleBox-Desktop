using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleBox.Helpers
{
    public static class CookieStorageHelper
    {
        #region Current

        public static CookieCollection CurrentCookies { get; set; } = LoadData();

        #endregion

        #region Storage Helper

        private static string GetConfigFileName() => Path.Combine(ConfigHelper.GetConfigFolder(), "cookies.xml");

        private static CookieCollection LoadData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CookieCollection));

            try
            {
                if (!File.Exists(GetConfigFileName())) throw new Exception();

                object data = serializer.Deserialize(new StringReader(File.ReadAllText(GetConfigFileName())));

                return data as CookieCollection;
            }
            catch (Exception)
            {
                return new CookieCollection();
            }
        }

        public static void SaveData()
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(CookieCollection));

            try
            {
                serializer.Serialize(stringWriter, CurrentCookies);

                File.WriteAllText(GetConfigFileName(), stringWriter.ToString());
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        #endregion
    }
}
