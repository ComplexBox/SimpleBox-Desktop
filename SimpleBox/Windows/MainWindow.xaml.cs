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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SimpleBox.Helpers;
using SimpleBox.Models;

namespace SimpleBox.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            Closing += (sender, args) =>
            {
                StorageHelper.SaveData(MallowSource.CurrentSource);
                ConfigHelper.SaveConfig(Config.Current);
                UpdateHelper.Current.Dispose();
            };

            Closed += (sender, args) => Application.Current.Shutdown(0);

            Loaded += OnLoaded;
        }

        #endregion

        #region Event Triggers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private void OpenSettingsButtonClick(object sender, RoutedEventArgs e) => SettingsWindow.ShowSettings();

        private void CreateMallowClick(object sender, RoutedEventArgs e)
        {
            if (MallowSource.CurrentSource.Current is null) return;
            Mallow mallow = new Mallow();
            MallowSource.CurrentSource.Current.Mallows.Add(mallow);
            MallowSource.CurrentSource.Current.CurrentMallow = mallow;
        }

        #endregion

        #region WndProc

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != 0x0084) return IntPtr.Zero;
            Point p = new Point();
            int pInt = lParam.ToInt32();
            p.X = (pInt << 16) >> 16;
            p.Y = pInt >> 16;
            Point rel = WndTitleArea.PointFromScreen(p);
            bool inside = PointFromScreen(p).Y <= 32 || rel.X >= 0 && rel.X <= WndTitleArea.ActualWidth && rel.Y >= 0 &&
                          rel.Y <= WndTitleArea.ActualHeight;
            if (!inside) return IntPtr.Zero;
            handled = true;
            return new IntPtr(2);
        }

        #endregion
    }
}
