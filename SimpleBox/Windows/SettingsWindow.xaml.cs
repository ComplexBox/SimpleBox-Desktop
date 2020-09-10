using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using SimpleBox.Helpers;

namespace SimpleBox.Windows
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
    {
        #region Current

        public static SettingsWindow Current { get; set; } = new SettingsWindow();

        public static void ShowSettings()
        {
            Current ??= new SettingsWindow();
            try
            {
                Current.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                Current = new SettingsWindow();
                Current.ShowDialog();
            }
        }

        #endregion

        #region Constructors

        public SettingsWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Processors

        private void UpdateButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateHelper.Current.TriggerProcess();
        }

        private void DebugButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button is null) return;

            switch (button.Tag)
            {
                case "CreateLoginWindow":
                    new LoginWindow("https://baidu.com").ShowDialog();
                    break;
            }
        }

        #endregion
    }
}
