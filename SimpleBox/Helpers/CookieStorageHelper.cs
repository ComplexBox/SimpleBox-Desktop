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

        public static CookieCollection CurrentCookies { get; set; } = new CookieCollection();

        #endregion
    }
}
