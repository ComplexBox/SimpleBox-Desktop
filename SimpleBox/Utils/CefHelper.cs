using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CefSharp;
using CefSharp.Handler;
using CefSharp.Wpf;
using SimpleBox.Helpers;
using WebSocketSharp;
using WebSocketSharp.Net;
using Cookie = System.Net.Cookie;

namespace SimpleBox.Utils
{
    public static class CefHelper
    {
        private static string GetLocaleName()
        {
            string n = CultureInfo.CurrentUICulture.Name.Split('-')[0];
            return n == "zh" || n == "en" || n == "pt" ? CultureInfo.CurrentUICulture.Name : n;
        }

        internal static Cookie[] ParseCookie(IResponse response)
        {
            CookieCollection cookieCollection = response.Headers.GetCookies(true);
            Cookie[] cookies = new Cookie[cookieCollection.Count];
            for (int i = 0; i < cookieCollection.Count; i++)
            {
                WebSocketSharp.Net.Cookie cookie = cookieCollection[i];
                cookies[i] = new Cookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Comment = cookie.Comment,
                    CommentUri = cookie.CommentUri,
                    Discard = cookie.Discard,
                    Domain = cookie.Domain,
                    Expired = cookie.Expired,
                    Expires = cookie.Expires,
                    HttpOnly = cookie.HttpOnly,
                    Path = cookie.Path,
                    Port = cookie.Port,
                    Secure = cookie.Secure,
                    Version = cookie.Version
                };
            }
            return cookies;
        }

        [STAThread]
        public static void Initialize()
        {
            CefSettings settings = new CefSettings();

            string cachePath = Path.Combine(ConfigHelper.GetConfigFolder(), "CefSharp\\Cache");
            Directory.CreateDirectory(cachePath);
            settings.CachePath = Path.Combine(cachePath);

            settings.Locale = GetLocaleName();
            settings.UserAgent =
                @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.8 Safari/537.36 Edg/86.0.622.3";

            Cef.EnableHighDPISupport();
            CefSharpSettings.ShutdownOnExit = false;

            Cef.Initialize(settings, true, browserProcessHandler: null);
        }

        [STAThread]
        public static void Shutdown() => Cef.Shutdown();
    }

    public sealed class MallowResourceRequestHandler : ResourceRequestHandler
    {
        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            Cookie[] cookie = CefHelper.ParseCookie(response);
            if (cookie.Any()) OnGetCookie?.Invoke(this, cookie);
        }

        public event EventHandler<Cookie[]> OnGetCookie;
    }

    public sealed class MallowRequestHandler : RequestHandler
    {
        private readonly MallowResourceRequestHandler _handler;

        public MallowRequestHandler(MallowResourceRequestHandler handler)
        {
            _handler = handler;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return _handler;
        }
    }
}
