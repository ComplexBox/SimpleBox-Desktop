// From: https://github.com/cqjjjzr/Marsher/blob/master/Marsher/Services.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SimpleBox.Core;
using SimpleBox.Models;
using SimpleBox.Utils.State;

namespace SimpleBox.Puller
{
    public abstract class MallowPuller : IMallowProvider
    {
        #region User Data

        public string Name { get; } = "";

        public Progress Progress { get; } = new Progress();

        #endregion

        #region Core Data

        protected const string HttpAccept =
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

        protected const string HttpAcceptEncoding = "UTF-8";

        protected readonly string HttpUserAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.8 Safari/537.36 Edg/86.0.622.3";

        #endregion

        #region Cookie Utils

        protected CookieContainer CookieContainer = new CookieContainer();

        protected string Cookie = "";

        protected bool TryLoadCookie() => Config.Current.UserTokens.TryGetValue(Name, out Cookie);

        protected void SaveCookie() => Config.Current.UserTokens[Name] = Cookie;

        #endregion

        #region Request Utils

        protected HttpWebRequest CreateWebRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = HttpUserAgent;
            request.Accept = HttpAccept;
            request.CookieContainer = CookieContainer;
            return request;
        }

        #endregion
    }
}
