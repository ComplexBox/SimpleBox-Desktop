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
    public class Mallow : INotifyPropertyChanged, IModified
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

        [JsonProperty]
        private long createTime;

        public long OriginalCreateTime
        {
            get => createTime;
            set
            {
                createTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private long responseTime;

        public long OriginalResponseTime
        {
            get => responseTime;
            set
            {
                responseTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Local Data Context

        private bool _isLocal;

        public bool IsLocal
        {
            get => _isLocal;
            private set
            {
                _isLocal = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private bool modified;

        public bool Modified
        {
            get => modified;
            set
            {
                modified = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private string localMsg = "";

        public string LocalMessage
        {
            get => localMsg;
            set
            {
                localMsg = value;
                Modified = true;
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
                Modified = true;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private long localCreateTime;

        public long LocalCreateTime
        {
            get => localCreateTime;
            set
            {
                localCreateTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private long localResponseTime;

        public long LocalResponseTime
        {
            get => localResponseTime;
            set
            {
                localResponseTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Serialization Methods

        [OnDeserialized]
        private void SetValuesOnDeserialized(StreamingContext context)
        {
            IsLocal = Id == LocalOnlyId;
            if (string.IsNullOrEmpty(LocalMessage))
                LocalMessage = OriginalMessage;
            if (string.IsNullOrEmpty(localResponse))
                LocalResponse = OriginalResponse;
            if (LocalCreateTime == 0)
                LocalCreateTime = OriginalCreateTime;
            if (LocalResponseTime == 0)
                LocalResponseTime = OriginalResponseTime;
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
