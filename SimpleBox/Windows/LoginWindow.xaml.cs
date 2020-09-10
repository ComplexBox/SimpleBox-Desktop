using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using SimpleBox.Puller;
using SimpleBox.Utils;
using Cookie = System.Net.Cookie;

namespace SimpleBox.Windows
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow
    {
        /// <summary>
        /// Open a new <see cref="LoginWindow"/> for test use.
        /// </summary>
        /// <param name="address">The start address.</param>
        public LoginWindow(string address)
        {
            InitializeComponent();

            Handler = new MallowResourceRequestHandler();
            MallowRequestHandler requestHandler = new MallowRequestHandler(Handler);
            Browser.RequestHandler = requestHandler;

            Browser.Address = address;
        }

        /// <summary>
        /// Create window for login.
        /// </summary>
        /// <param name="puller">The mallow puller.</param>
        public LoginWindow(MallowPuller puller)
        {
            InitializeComponent();

            Handler = new MallowResourceRequestHandler();

            Handler.OnGetCookie += (sender, cookies) =>
            {
                foreach (Cookie cookie in cookies) puller.CookieContainer.Add(cookie);

                CookieCollection rawCollection = puller.CookieContainer.GetCookies(puller.Host);
                Cookie[] cookieArr = new Cookie[rawCollection.Count];
                rawCollection.CopyTo(cookieArr, 0);

                if (puller.CookieChecks.Any(check => cookieArr.All(cookie => cookie.Name != check)))
                    return;

                Close();
            };

            MallowRequestHandler requestHandler = new MallowRequestHandler(Handler);
            Browser.RequestHandler = requestHandler;

            Browser.Address = puller.LoginAddress;
        }

        public MallowResourceRequestHandler Handler { get; }
    }
}
