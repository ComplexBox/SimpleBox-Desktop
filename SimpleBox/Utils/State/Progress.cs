using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SimpleBox.Utils.State
{
    public class Progress : INotifyPropertyChanged
    {
        #region Data Context

        private string _text = "就绪";

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndeterminate;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        private double _percentage;

        public double Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a <see cref="Progress"/> class.
        /// </summary>
        public Progress()
        {

        }

        /// <summary>
        /// Create a <see cref="Progress"/> class for display use.
        /// <para>
        /// This will automatically add an event listener on the <paramref name="source"/> object
        /// to trigger <paramref name="dispatcher"/> react on the changes.
        /// </para>
        /// </summary>
        /// <param name="source">The source class.</param>
        /// <param name="dispatcher">The dispatcher of the window.</param>
        public Progress(Progress source, Dispatcher dispatcher)
        {
            if (source is null || dispatcher is null) return;
            source.PropertyChanged += (sender, args) =>
            {
                dispatcher.Invoke(() =>
                {
                    Text = source.Text;
                    IsIndeterminate = source.IsIndeterminate;
                    Percentage = source.Percentage;
                });
            };
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
