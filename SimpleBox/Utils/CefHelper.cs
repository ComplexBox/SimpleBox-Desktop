﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;
using CefSharp.Wpf;
using SimpleBox.Helpers;

namespace SimpleBox.Utils
{
    public static class CefHelper
    {
        private static RequestHandler _requestHandler;

        public static RequestHandler RequestHandler => _requestHandler ??= new MallowRequestHandler();

        private static string GetLocaleName()
        {
            string n = CultureInfo.CurrentUICulture.Name.Split('-')[0];
            return n == "zh" || n == "en" || n == "pt" ? CultureInfo.CurrentUICulture.Name : n;
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

    public sealed class MallowRequestHandler : RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture,
            bool isRedirect)
        {
            request.SetHeaderByName("Accept-Language", CultureInfo.CurrentUICulture.Name, true);
            return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }
    }
}