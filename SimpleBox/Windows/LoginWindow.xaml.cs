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
        public LoginWindow(string address)
        {
            InitializeComponent();

            Handler = new MallowResourceRequestHandler();
            MallowRequestHandler requestHandler = new MallowRequestHandler(Handler);
            Browser.RequestHandler = requestHandler;

            Browser.Address = address;
        }

        public MallowResourceRequestHandler Handler { get; }
    }
}
