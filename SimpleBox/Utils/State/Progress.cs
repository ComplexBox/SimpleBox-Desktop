using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBox.Utils.State
{
    public class Progress : INotifyPropertyChanged
    {
        #region Data Context

        private ProgressState _state = ProgressState.Waiting;

        public ProgressState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
                StateChanged?.Invoke(this, value);
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private double _percent;

        public double Percent
        {
            get => _percent;
            set
            {
                _percent = value;
                OnPropertyChanged();
                PercentChanged?.Invoke(this, value);
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Events

        public event EventHandler<ProgressState> StateChanged;

        public event EventHandler<double> PercentChanged;

        public event EventHandler ProgressChanged;

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public enum ProgressState
    {
        Waiting = 0,
        Working = 1,
        Success = 2,
        Failed = 3
    }
}
