﻿// From: https://github.com/cqjjjzr/Marsher/blob/master/Marsher/Services.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using SimpleBox.Core;
using SimpleBox.Helpers;
using SimpleBox.Models;
using SimpleBox.Utils.Cef;
using SimpleBox.Utils.State;
using SimpleBox.Windows;

namespace SimpleBox.Puller
{
    public abstract class MallowPuller : IMallowProvider
    {
        #region User Data

        public abstract string Name { get; }

        public abstract string Address { get; }

        public Progress Progress { get; } = new Progress();

        #endregion

        #region Core Data

        protected const string HttpAccept =
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

        protected readonly string HttpUserAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.8 Safari/537.36 Edg/86.0.622.3";

        #endregion

        #region Request Utils

        protected async Task<HttpWebRequest> CreateWebRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = HttpUserAgent;
            request.Accept = HttpAccept;
            request.CookieContainer = new CookieContainer();
            await CefHelper.CollectCookies();
            //request.CookieContainer.Add(CookieStorageHelper.CurrentCookies);

            foreach (Cookie cookie in CookieStorageHelper.CurrentCookies)
            {
                try
                {
                    request.CookieContainer.Add(cookie);
                }
                catch (CookieException)
                {
                    // Ignore
                }
            }

            return request;
        }

        #endregion

        #region Verify Utils

        public async Task<bool> VerifyLogin()
        {
            HttpWebRequest request = await CreateWebRequest(Address);
            request.AllowAutoRedirect = false;
            try
            {
                return request.GetResponse() is HttpWebResponse response && response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Mallow[]> VerifyAndPull(Mallow[] existingMallows)
        {
            if (await VerifyLogin()) return await Pull(existingMallows);

            bool isLoginComplete = Application.Current.Dispatcher.Invoke(() =>
            {
                LoginWindow loginWindow = new LoginWindow(this);
                loginWindow.ShowDialog();

                return loginWindow.IsLoginComplete;
            });

            if (!isLoginComplete) return null;

            if (await VerifyLogin()) return await Pull(existingMallows);

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

        protected abstract Task<Mallow[]> Pull(Mallow[] existingMallows);

        #endregion
    }
}
