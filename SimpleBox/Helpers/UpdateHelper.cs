using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SimpleBox.Models;
using Squirrel;

namespace SimpleBox.Helpers
{
    public class UpdateHelper : IDisposable, INotifyPropertyChanged
    {
        #region Manager

        private UpdateManager _updateManager;

        #endregion

        #region Current

        public static UpdateHelper Current { get; } = InitializeUpdateHelper();

        #endregion

        #region Constructor

        private UpdateHelper()
        {
        }

        public static UpdateHelper InitializeUpdateHelper()
        {
            UpdateHelper helper = new UpdateHelper();
            helper.ReconstructUpdateManager();
            helper.TriggerProcess();
            return helper;
        }

        #endregion

        #region Processors

        private void ReconstructUpdateManager(bool triggerReady = true)
        {
            _updateManager?.Dispose();
            _updateManager = new UpdateManager(Config.Current.UpdateServer);
            if (triggerReady) UpdateMode = "ready";
        }

        public void TriggerProcess()
        {
            if (UpdateMode != "ready" && UpdateMode != "error") return;
            ReconstructUpdateManager(false);
            UpdateMode = "search";
            try
            {
                Task.Run(async () =>
                {
                    UpdateInfo updateInfo = await _updateManager.CheckForUpdate(false,
                        progress => Application.Current.Dispatcher.Invoke(() => UpdateProgress = progress));
                    Application.Current.Dispatcher.Invoke(() => UpdateProgress = 100);
                    if (!updateInfo.ReleasesToApply.Any())
                    {
                        Application.Current.Dispatcher.Invoke(() => UpdateMode = "ready");
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() => UpdateMode = "down");

                    await _updateManager.DownloadReleases(updateInfo.ReleasesToApply,
                        progress => Application.Current.Dispatcher.Invoke(() => UpdateProgress = progress));

                    Application.Current.Dispatcher.Invoke(() => UpdateMode = "inst");

                    await _updateManager.ApplyReleases(updateInfo,
                        progress => Application.Current.Dispatcher.Invoke(() => UpdateProgress = progress));

                    Application.Current.Dispatcher.Invoke(() => UpdateMode = "restart");
                });
            }
            catch (Exception)
            {
                UpdateMode = "error";
            }
        }

        #endregion

        #region DataContext

        private string _updateMode = "ready";

        /// <summary>
        ///     The Update Mode.
        ///     Possible values:
        ///     ready | search | down | inst | restart | error
        /// </summary>
        public string UpdateMode
        {
            get => _updateMode;
            set
            {
                _updateMode = value;
                UpdateProgress = 0;
                OnPropertyChanged();
            }
        }

        private int _updateProgress;

        public int UpdateProgress
        {
            get => _updateProgress;
            set
            {
                _updateProgress = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }
}
