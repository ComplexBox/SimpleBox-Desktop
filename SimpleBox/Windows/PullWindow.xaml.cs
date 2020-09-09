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
using SimpleBox.Puller;
using SimpleBox.Utils.State;

namespace SimpleBox.Windows
{
    /// <summary>
    /// PullWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PullWindow
    {
        #region Constructors

        public PullWindow(IMallowPuller puller)
        {
            // Create Progress for Display

            Progress = new Progress(puller.Progress, Dispatcher);

            // DataContext Initialize

            PullerName = puller.Name;

            // Initialize Component

            InitializeComponent();
        }

        #endregion

        #region Data Context

        public string PullerName { get; }

        public Progress Progress { get; }

        #endregion
    }
}
