using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SimpleBox.Core;
using SimpleBox.Helpers;
using SimpleBox.Models;
using SimpleBox.Utils;
using SimpleBox.Windows;
using SourceChord.FluentWPF;
using Squirrel;

namespace SimpleBox
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Exception Handler

            DispatcherUnhandledException += (sender, args) =>
            {
                args.Handled = true;
                MessageBox.Show(
                    args.Exception.Message,
                    "灾难性故障",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                MessageBox.Show(
                    ((Exception)args.ExceptionObject)?.Message ?? "Exception",
                    "灾难性故障",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            };

            // SquirrelAware

            using (UpdateManager mgr = new UpdateManager(Config.Current.UpdateServer))
            {
                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: v =>
                    {
                        mgr.CreateShortcutForThisExe();
                        Current.Shutdown(0);
                    },
                    onAppUpdate: v =>
                    {
                        mgr.CreateShortcutForThisExe();
                        Current.Shutdown(0);
                    },
                    onAppUninstall: v =>
                    {
                        mgr.RemoveShortcutForThisExe();
                        Current.Shutdown(0);
                    });
            }

            SimpleBox.Properties.Resources.Culture = CultureInfo.CurrentUICulture;

            // Initialize UpdateHelper

            string updateMode = UpdateHelper.Current.UpdateMode;

            // Initialize WebPush

            WebPush.Current = new WebPush();
            WebPush.Current.Start();

            if (!WebPush.Current.IsListening)
            {
                MessageBox.Show(
                    "信息推送服务无法启动。请检查端口占用。程序即将退出。",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
                Current.Shutdown(1);
            }

            // Initialize CEFSharp

            CefHelper.Initialize();

            // Force Dark Theme

            ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;

            // Create MainWindow

            MainWindow ??= new MainWindow();

            // Show MainWindow

            MainWindow.Show();
        }
    }
}
