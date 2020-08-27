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
        }

        #endregion

        #region Event Triggers

        private void OpenSettingsButtonClick(object sender, RoutedEventArgs e) => SettingsWindow.ShowSettings();

        #endregion

        private void CreateMallowClick(object sender, RoutedEventArgs e)
        {
            if (MallowSource.CurrentSource.Current is null) return;
            Mallow mallow = new Mallow();
            MallowSource.CurrentSource.Current.Mallows.Add(mallow);
            MallowSource.CurrentSource.Current.CurrentMallow = mallow;
        }
    }
}
