// From: https://github.com/cqjjjzr/Marsher/blob/master/Marsher/Services.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using SimpleBox.Core;
using SimpleBox.Models;
using SimpleBox.Utils.State;
using SimpleBox.Windows;

namespace SimpleBox.Puller
{
    public abstract class MallowPuller : IMallowProvider
    {
        #region User Data

        public string Name { get; }

        public Uri Host;

        public string LoginAddress;

        protected string VerifyAddress;

        public Progress Progress { get; } = new Progress();

        #endregion

        #region Core Data

        protected const string HttpAccept =
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

        protected readonly string HttpUserAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.8 Safari/537.36 Edg/86.0.622.3";

        #endregion

        #region Cookie Utils

        public CookieContainer CookieContainer = new CookieContainer();

        public string[] CookieChecks = new string[0];

        protected bool TryLoadCookie()
        {
            if (!Config.Current.UserTokens.TryGetValue(Name, out string raw) ||
                string.IsNullOrEmpty(raw)) return false;

            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));

            try
            {
                object data = serializer.Deserialize(new StringReader(raw));
                CookieContainer = data as CookieContainer;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected void SaveCookie()
        {
            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(CookieContainer));

            try
            {
                serializer.Serialize(stringWriter, CookieContainer);
            }
            catch (Exception)
            {
                Config.Current.UserTokens.Remove(Name);
                return;
            }

            Config.Current.UserTokens[Name] = stringWriter.ToString();
        }

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

        #region Verify Utils

        private bool VerifyLogin()
        {
            HttpWebRequest request = CreateWebRequest(VerifyAddress);
            return request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK;
        }

        public Mallow[] VerifyAndPull(Mallow[] existingMallows)
        {
            if (VerifyLogin()) return Pull(existingMallows);

            LoginWindow loginWindow = new LoginWindow(this);
            loginWindow.ShowDialog();
            if (!loginWindow.IsLoginComplete) return null;

            if (VerifyLogin()) return Pull(existingMallows);

            MessageBox.Show(
                $"无法登录到{Name}服务，因此无法拉取。",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
            return null;
        }

        #endregion

        #region Interface

        protected abstract Mallow[] Pull(Mallow[] existingMallows);

        #endregion
    }
}
