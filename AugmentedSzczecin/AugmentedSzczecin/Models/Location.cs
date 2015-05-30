using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{

    public class Location : PropertyChangedBase
    {
        [JsonProperty(PropertyName = "latitude")]
        private double _latitude;
        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyOfPropertyChange(() => Latitude);
                }
            }
        }

        [JsonProperty(PropertyName = "longitude")]
        private double _longitude;
        public double Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyOfPropertyChange(() => Longitude);
                }
            }
        }
    }
}