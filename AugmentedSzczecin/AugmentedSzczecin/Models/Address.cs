using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{

    public class Address : PropertyChangedBase
    {
        [JsonProperty(PropertyName = "city")]
        private string _city;
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    NotifyOfPropertyChange(() => City);
                }
            }
        }

        [JsonProperty(PropertyName = "street")]
        private string _street;
        public string Street
        {
            get
            {
                return _street;
            }
            set
            {
                if (_street != value)
                {
                    _street = value;
                    NotifyOfPropertyChange(() => Street);
                }
            }
        }

        [JsonProperty(PropertyName = "streetNumber")]
        private string _streetNumber;
        public string StreetNumber
        {
            get
            {
                return _streetNumber;
            }
            set
            {
                if (_streetNumber != value)
                {
                    _streetNumber = value;
                    NotifyOfPropertyChange(() => StreetNumber);
                }
            }
        }

        [JsonProperty(PropertyName = "zipcode")]
        private string _zipCode;
        public string ZipCode
        {
            get
            {
                return _zipCode;
            }
            set
            {
                if (_zipCode != value)
                {
                    _zipCode = value;
                    NotifyOfPropertyChange(() => ZipCode);
                }
            }
        }
    }
}