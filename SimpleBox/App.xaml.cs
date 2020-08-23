using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SimpleBox.Windows;
using SourceChord.FluentWPF;

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

            SimpleBox.Properties.Resources.Culture = CultureInfo.CurrentUICulture;

            // Force Dark Theme

            ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;

            // Create MainWindow

            MainWindow ??= new MainWindow();

            // Show MainWindow

            MainWindow.Show();
        }
    }
}
