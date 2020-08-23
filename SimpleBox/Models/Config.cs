using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBox.Helpers;

namespace SimpleBox.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config : INotifyPropertyChanged
    {
        #region Current

        public static Config Current { get; } = ConfigHelper.OpenConfig();

        #endregion

        #region Update

        [JsonProperty]
        private string updateServer = "https://simplebox.vbox.moe/stable";

        public string UpdateServer
        {
            get => updateServer;
            set
            {
                updateServer = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region User Data

        [JsonProperty]
        private string username = "";

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private string password = "";

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
