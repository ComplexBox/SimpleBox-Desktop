using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SimpleBox.Models;
using SimpleBox.Puller;
using SimpleBox.Utils.State;

namespace SimpleBox.Windows
{
    /// <summary>
    /// PullWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PullWindow : INotifyPropertyChanged
    {
        #region Constructors

        public PullWindow(MallowPuller puller, EventWaitHandle handle)
        {
            // Create Progress for Display

            Progress = new Progress(puller.Progress, Dispatcher);

            // DataContext Initialize

            PullerName = puller.Name;
            CreateGroupName = $"{puller.Name}导入";

            Handle = handle;
            Puller = puller;

            // Initialize Component

            InitializeComponent();
        }

        #endregion

        #region Core

        public readonly EventWaitHandle Handle;

        public readonly MallowPuller Puller;

        #endregion

        #region Data Context

        public string PullerName { get; }

        public Progress Progress { get; }

        private MallowGroup _selectedGroup;

        public MallowGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged();
            }
        }

        private string _createGroupName;

        public string CreateGroupName
        {
            get => _createGroupName;
            set
            {
                _createGroupName = value;
                OnPropertyChanged();
            }
        }

        private bool _isCreateMode;

        public bool IsCreateMode
        {
            get => _isCreateMode;
            set
            {
                _isCreateMode = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Event Processors

        private void StartClick(object sender, RoutedEventArgs e)
        {
            RootElement.IsEnabled = false;
            Handle?.Set();
        }

        private void CancelClick(object sender, RoutedEventArgs e) => Close();

        private void ModeChangeClick(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio?.Tag != null) IsCreateMode = radio.Tag as string == "CreateModeRadioButton";
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
