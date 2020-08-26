using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleBox.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Mallow : INotifyPropertyChanged
    {
        #region Const Data

        private const string LocalOnlyId = "LOCAL_ONLY";

        #endregion
        #region Online Data Context

        [JsonProperty]
        private MallowStatus status = 0;

        public MallowStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private string id = LocalOnlyId;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                _isLocal = value == LocalOnlyId;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLocal));
            }
        }

        [JsonProperty]
        private string msg = "";

        public string OriginalMessage
        {
            get => msg;
            set
            {
                msg = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private string response = "";

        public string OriginalResponse
        {
            get => response;
            set
            {
                response = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Local Data Context

        private bool _isLocal;

        public bool IsLocal => _isLocal;

        [JsonProperty]
        private string localMsg = "";

        public string LocalMessage
        {
            get => localMsg;
            set
            {
                localMsg = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private string localResponse = "";

        public string LocalResponse
        {
            get => localResponse;
            set
            {
                localResponse = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Serialization Methods

        [OnDeserialized]
        private void SetValuesOnDeserialized(StreamingContext context)
        {
            if (string.IsNullOrEmpty(LocalMessage))
                LocalMessage = OriginalMessage;
            if (string.IsNullOrEmpty(localResponse))
                LocalResponse = OriginalResponse;
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

    public enum MallowStatus
    {
        Unread = 0,
        Read,
        Answered
    }
}
