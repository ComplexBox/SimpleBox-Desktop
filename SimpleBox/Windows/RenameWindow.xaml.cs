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

namespace SimpleBox.Windows
{
    /// <summary>
    /// RenameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RenameWindow
    {
        private RenameWindow()
        {
            InitializeComponent();
        }

        public static string Rename(string origin)
        {
            RenameWindow window = new RenameWindow
            {
                RenameTextBox =
                {
                    Text = origin
                }
            };
            window.ShowDialog();
            return window.RenameTextBox.Text;
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e) => Close();

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            RenameTextBox.Text = "";
            Close();
        }

        private void RenameTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Close();
        }
    }
}
