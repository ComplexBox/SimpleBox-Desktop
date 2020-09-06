using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBox.Helpers
{
    public static class NetworkHelper
    {
        public static string DownloadString(string url)
        {
            WebRequest request = WebRequest.CreateHttp(url);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream()!);
            return reader.ReadToEnd();
        }
    }
}
