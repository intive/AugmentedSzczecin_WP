using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{

    public class Opening : PropertyChangedBase
    {
        [JsonProperty(PropertyName = "day")]
        private string _day;
        public string Day
        {
            get
            {
                return _day;
            }
            set
            {
                if (_day != value)
                {
                    _day = value;
                    NotifyOfPropertyChange(() => Day);
                }
            }
        }

        [JsonProperty(PropertyName = "open")]
        private string _open;
        public string Open
        {
            get
            {
                return _open;
            }
            set
            {
                if (_open != value)
                {
                    _open = value;
                    NotifyOfPropertyChange(() => Open);
                }
            }
        }

        [JsonProperty(PropertyName = "close")]
        private string _close;
        public string Close
        {
            get
            {
                return _close;
            }
            set
            {
                if (_close != value)
                {
                    _close = value;
                    NotifyOfPropertyChange(() => Close);
                }
            }
        }
    }
}