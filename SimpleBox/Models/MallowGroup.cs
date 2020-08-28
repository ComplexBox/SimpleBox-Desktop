using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBox.Helpers;
using SimpleBox.Utils;

namespace SimpleBox.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MallowSource : INotifyPropertyChanged
    {
        #region Current

        public static MallowSource CurrentSource { get; set; } = StorageHelper.LoadData();

        #endregion

        #region Data

        [JsonProperty]
        private ObservableCollection<MallowGroup> data = new ObservableCollection<MallowGroup>();

        public ObservableCollection<MallowGroup> Data => data;

        private MallowGroup _current;

        public MallowGroup Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Serialization Methods

        [OnDeserialized]
        public void SetValuesOnDeserialized(StreamingContext context)
        {
            if (!Data.Any())
                Data.Add(new MallowGroup
                {
                    Name = "默认分组"
                });
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

    [JsonObject(MemberSerialization.OptIn)]
    public class MallowGroup : INotifyPropertyChanged
    {
        #region Data

        [JsonProperty]
        private string name = $"{DateTime.Now:MM-dd}时创建的分组";

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        private ObservableCollection<Mallow> mallows = new ObservableCollection<Mallow>();

        public ObservableCollection<Mallow> Mallows => mallows;

        private Mallow _currentMallow;

        public Mallow CurrentMallow
        {
            get => _currentMallow;
            set
            {
                _currentMallow = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        [JsonConverter(typeof(MallowDateTimeConverter))]
        private DateTime? modifiedTime = DateTime.Now;

        public DateTime? ModifiedTime
        {
            get => modifiedTime;
            set
            {
                modifiedTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public MallowGroup() => Mallows.CollectionChanged += OnMallowCollectionChanged;

        #endregion

        #region Utilities

        private void TriggerModified() => ModifiedTime = DateTime.Now;

        private void OnMallowCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add &&
                e.Action != NotifyCollectionChangedAction.Replace) return;

            foreach (object eItem in e.NewItems)
            {
                Mallow item = eItem as Mallow;
                if (item is null) continue;
                item.PropertyChanged += OnMallowPropertyChanged;
            }
        }

        private void OnMallowPropertyChanged(object sender, PropertyChangedEventArgs e) => TriggerModified();

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
