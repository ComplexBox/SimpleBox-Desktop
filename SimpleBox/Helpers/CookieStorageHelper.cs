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

        public static CookieContainer CurrentCookieContainer { get; } = LoadData();

        #endregion

        #region Storage Helper

        private static string GetConfigFileName() => Path.Combine(ConfigHelper.GetConfigFolder(), "cookies.xml");

        private static CookieContainer LoadData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));

            try
            {
                if (!File.Exists(GetConfigFileName())) throw new Exception();

                object data = serializer.Deserialize(new StringReader(File.ReadAllText(GetConfigFileName())));
                return data as CookieContainer;
            }
            catch (Exception)
            {
                return new CookieContainer();
            }
        }

        public static void SaveData()
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));

            try
            {
                serializer.Serialize(stringWriter, CurrentCookieContainer);

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
