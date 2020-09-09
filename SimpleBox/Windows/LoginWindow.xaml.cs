using System;
using System.Collections.Generic;
using System.Linq;
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
using SimpleBox.Utils;

namespace SimpleBox.Windows
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            MallowResourceRequestHandler handler = new MallowResourceRequestHandler();
            MallowRequestHandler requestHandler = new MallowRequestHandler(handler);
            Browser.RequestHandler = requestHandler;
            handler.OnResponse += BrowserHandlerOnOnResponse;
        }

        private void BrowserHandlerOnOnResponse(object sender, KeyValuePair<IResponse, byte[]> e)
        {
            IResponse response = e.Key;
            byte[] data = e.Value;
        }
    }
}
