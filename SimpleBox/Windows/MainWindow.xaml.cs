using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SimpleBox.Utils;
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

            Closing += OnClosing;

            Closed += (sender, args) => Application.Current.Shutdown(0);

            Loaded += OnLoaded;
        }

        #endregion

        #region Lifecycle Events

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            StorageHelper.SaveData(MallowSource.CurrentSource);
            ConfigHelper.SaveConfig(Config.Current);
            WebPush.Current.Stop();
            CefHelper.Shutdown();
            UpdateHelper.Current.Dispose();
        }

        #endregion

        #region Triggers - Open Extra Window

        private void OpenSettingsButtonClick(object sender, RoutedEventArgs e) => SettingsWindow.ShowSettings();

        private void OpenSyncSettingsPopupClick(object sender, RoutedEventArgs e) =>
            ShowPopup(SyncSettingsPopup, SyncSettingsButton);

        #endregion

        #region Triggers - Mallow Modifitions

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

            if (ConfirmDelete()) MallowSource.CurrentSource.Data.Remove(group);
        }

        private void MallowDeleteClick(object sender, RoutedEventArgs e)
        {
            if (!((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as FrameworkElement)?.DataContext is Mallow mallow)) return;

            if (ConfirmDelete()) MallowSource.CurrentSource.Current.Mallows.Remove(mallow);
        }

        private void MallowMultiDeleteClick(object sender, RoutedEventArgs e)
        {
            if (MallowList.SelectedItems.Count == 0 || MallowSource.CurrentSource.Current is null || !ConfirmDelete()) return;

            Mallow[] mallows = new Mallow[MallowList.SelectedItems.Count];
            MallowList.SelectedItems.CopyTo(mallows, 0);

            foreach (Mallow mallow in mallows)
            {
                if (mallow is null || !MallowSource.CurrentSource.Current.Mallows.Contains(mallow)) continue;
                MallowSource.CurrentSource.Current.Mallows.Remove(mallow);
            }
        }

        private void MallowMoveClick(object sender, RoutedEventArgs e)
        {
            Mallow source =
                ((sender as MenuItem)?.CommandParameter as FrameworkElement)?.DataContext as Mallow;
            MallowGroup sourceGroup = MallowSource.CurrentSource.Current;
            MallowGroup targetGroup = (sender as MenuItem)?.DataContext as MallowGroup;
            
            if (source is null ||
                sourceGroup is null ||
                targetGroup is null ||
                sourceGroup == targetGroup ||
                !sourceGroup.Mallows.Contains(source)) return;

            targetGroup.Mallows.Insert(0, source);
            sourceGroup.Mallows.Remove(source);
        }

        private void MallowMultiMoveClick(object sender, RoutedEventArgs e)
        {
            MallowGroup sourceGroup = MallowSource.CurrentSource.Current;
            MallowGroup targetGroup = (sender as MenuItem)?.DataContext as MallowGroup;

            if (MallowList.SelectedItems.Count == 0 ||
                sourceGroup is null ||
                targetGroup is null ||
                sourceGroup == targetGroup) return;

            Mallow[] mallows = new Mallow[MallowList.SelectedItems.Count];
            MallowList.SelectedItems.CopyTo(mallows, 0);

            for (int i = 0; i < mallows.Length; i++)
            {
                Mallow mallow = mallows[i];
                if (!sourceGroup.Mallows.Contains(mallow)) continue;
                targetGroup.Mallows.Insert(i, mallow);
                sourceGroup.Mallows.Remove(mallow);
            }
        }

        #endregion

        #region Triggers - WebPush

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

        #endregion

        #region Triggers - Import & Export

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            ImportExportHelper.Import(button.Tag switch
            {
                "SimpleBox" => new SimpleBoxImporter(),
                "Marsher" => new MarsherImporter(),
                _ => null
            });
        }

        private void ExportGroupClick(object sender, RoutedEventArgs e)
        {
            if (MallowSource.CurrentSource.Current != null && MallowSource.CurrentSource.Current.Mallows.Any())
                ImportExportHelper.Export(MallowSource.CurrentSource.Current.Mallows.ToList());
        }

        #endregion

        #region Triggers - MultiSelect

        private void ToggleMultiSelectClick(object sender, RoutedEventArgs e) => IsSelecting = !IsSelecting;

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            MallowList?.SelectedItems.Clear();
            if (MallowSource.CurrentSource.Current is null) return;
            foreach (Mallow mallow in MallowSource.CurrentSource.Current.Mallows)
                MallowList?.SelectedItems.Add(mallow);
        }

        private void SelectAllOffClick(object sender, RoutedEventArgs e) => MallowList?.SelectedItems.Clear();

        #endregion

        #region Utilities

        private static void ShowPopup(Popup popup, UIElement placementTarget)
        {
            popup.IsOpen = false;
            popup.PlacementTarget = placementTarget;
            popup.IsOpen = true;
        }

        private static bool ConfirmDelete()
        {
            return MessageBox.Show(
                "确定要删除吗?",
                "删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.Yes) == MessageBoxResult.Yes;
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
