using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SimpleBox.Models;

namespace SimpleBox.Windows
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Data Context

        private bool _isSelecting;

        public bool IsSelecting
        {
            get => _isSelecting;
            set
            {
                _isSelecting = value;
                SelectedMallows?.Clear();
                OnPropertyChanged();
            }
        }

        private IList<Mallow> _selectedMallows;

        public IList<Mallow> SelectedMallows
        {
            get => _selectedMallows;
            set
            {
                _selectedMallows = value;
                OnPropertyChanged();
            }
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
