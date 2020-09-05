using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using SimpleBox.Core;
using SimpleBox.Helpers;
using SimpleBox.Models;
using SourceChord.FluentWPF;

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
                WebPush.Current.Stop();
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
            mallow.SetValuesOnDeserialized(new StreamingContext());
            MallowSource.CurrentSource.Current.Mallows.Insert(0, mallow);
            MallowSource.CurrentSource.Current.CurrentMallow = mallow;
        }

        private void CreateMallowGroupClick(object sender, RoutedEventArgs e)
        {
            MallowGroup group = new MallowGroup
            {
                Name = "新分组",
                ModifiedTime = DateTime.Now
            };
            MallowSource.CurrentSource.Data.Insert(0, group);
            MallowSource.CurrentSource.Current = group;
        }

        private void OpenSyncSettingsPopupClick(object sender, RoutedEventArgs e) =>
            ShowPopup(SyncSettingsPopup, SyncSettingsButton);

        private void GroupRenameClick(object sender, RoutedEventArgs e)
        {
            if (!((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as FrameworkElement)?.DataContext is MallowGroup group)) return;

            string result = RenameWindow.Rename(group.Name);
            if (!string.IsNullOrEmpty(result))
                group.Name = result;
        }

        private void GroupDeleteClick(object sender, RoutedEventArgs e)
        {
            if (!((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as FrameworkElement)?.DataContext is MallowGroup group)) return;

            MallowSource.CurrentSource.Data.Remove(group);
        }

        private void MallowDeleteClick(object sender, RoutedEventArgs e)
        {
            if (!((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as FrameworkElement)?.DataContext is Mallow mallow)) return;

            MallowSource.CurrentSource.Current.Mallows.Remove(mallow);
        }

        private void WebPushClick(object sender, RoutedEventArgs e)
        {
            WebPushTextBlock.Text = "正在推送……";
            if (MallowSource.CurrentSource.Current is null ||
                MallowSource.CurrentSource.Current.CurrentMallow is null)
            {
                WebPushTextBlock.Text = "显示";
                return;
            }

            WebPush.Current.PushMallow(
                MallowSource.CurrentSource.Current.CurrentMallow,
                PushMallowCompleted);
        }

        private async void PushMallowCompleted() => await Dispatcher.InvokeAsync(() => WebPushTextBlock.Text = "已显示");

        private void ImportFromSimpleBoxManagerClick(object sender, RoutedEventArgs e) =>
            ImportHelper.Import(new ManagerImporter());

        #endregion

        #region Utilities

        private static void ShowPopup(Popup popup, UIElement placementTarget)
        {
            popup.IsOpen = false;
            popup.PlacementTarget = placementTarget;
            popup.IsOpen = true;
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
            bool inside = rel.X >= 0 && rel.X <= WndTitleArea.ActualWidth && rel.Y >= 0 &&
                          rel.Y <= WndTitleArea.ActualHeight;
            if (!inside) return IntPtr.Zero;
            handled = true;
            return new IntPtr(2);
        }

        #endregion
    }
}
