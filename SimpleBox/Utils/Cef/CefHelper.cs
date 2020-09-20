using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;
using CefSharp.Wpf;
using SimpleBox.Helpers;
using Cookie = System.Net.Cookie;

namespace SimpleBox.Utils.Cef
{
    public static class CefHelper
    {
        private static string GetLocaleName()
        {
            string n = CultureInfo.CurrentUICulture.Name.Split('-')[0];
            return n == "zh" || n == "en" || n == "pt" ? CultureInfo.CurrentUICulture.Name : n;
        }

        internal static CookieCollection ParseCookie(IRequest request, IResponse response) =>
            CookieHelper.Current.ParseSetCookie(response.Headers["Set-Cookie"],
                CookieHelper.ExtractDomain(request.Url));

        public static async Task CollectCookies()
        {
            ICookieManager cookieManager = CefSharp.Cef.GetGlobalCookieManager();
            TaskCookieVisitor cookieVisitor = new TaskCookieVisitor();

            cookieManager.VisitAllCookies(cookieVisitor);

            List<CefSharp.Cookie> cookies = await cookieVisitor.Task;

            foreach (Cookie c in cookies.Select(ConvertCookie).Where(c => c != null))
            {
                CookieStorageHelper.CurrentCookieContainer.Add(c);
            }
        }

        private static Cookie ConvertCookie(CefSharp.Cookie cookie)
        {
            return cookie.Expires != null
                ? new Cookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Expires = (DateTime) cookie.Expires,
                    HttpOnly = cookie.HttpOnly,
                    Path = cookie.Path,
                    Secure = cookie.Secure,
                }
                : null;
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

            CefSharp.Cef.EnableHighDPISupport();
            CefSharpSettings.ShutdownOnExit = false;

            CefSharp.Cef.Initialize(settings, true, browserProcessHandler: null);
        }

        [STAThread]
        public static void Shutdown() => CefSharp.Cef.Shutdown();
    }

    public sealed class MallowResourceRequestHandler : ResourceRequestHandler
    {
        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            CookieCollection cookies = CefHelper.ParseCookie(request, response);
            if (cookies.Count > 0) OnGetCookie?.Invoke(this, cookies);
        }

        public event EventHandler<CookieCollection> OnGetCookie;
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
