using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{
    public class PointOfInterest : PropertyChangedBase
    {
        public PointOfInterest(string id)
        {
            Id = id;
        }
        public string Id { get; private set; }

        [JsonProperty(PropertyName = "Name")]
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        [JsonProperty(PropertyName = "Description")]
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    NotifyOfPropertyChange(() => Description);
                }
            }
        }

        [JsonProperty(PropertyName = "Tags")]
        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    NotifyOfPropertyChange(() => Tags);
                }
            }
        }

        [JsonProperty(PropertyName = "Latitude")]
        private double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyOfPropertyChange(() => Latitude);
                }
            }
        }

        [JsonProperty(PropertyName = "Longitude")]
        private double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyOfPropertyChange(() => Longitude);
                }
            }
        }

        [JsonProperty(PropertyName = "StreetName")]
        private string _streetName;
        public string StreetName
        {
            get { return _streetName; }
            set
            {
                if (_streetName != value)
                {
                    _streetName = value;
                    NotifyOfPropertyChange(() => StreetName);
                }
            }
        }

        [JsonProperty(PropertyName = "PostalCode")]
        private string _postalCode;
        public string PostalCode
        {
            get { return _postalCode; }
            set
            {
                if (_postalCode != value)
                {
                    _postalCode = value;
                    NotifyOfPropertyChange(() => PostalCode);
                }
            }
        }

        [JsonProperty(PropertyName = "Country")]
        private string _country;
        public string Country
        {
            get { return _country; }
            set
            {
                if (_country != value)
                {
                    _country = value;
                    NotifyOfPropertyChange(() => Country);
                }
            }
        }

        [JsonProperty(PropertyName = "Website")]
        private string _website;
        public string Website
        {
            get { return _website; }
            set
            {
                if (_website != value)
                {
                    _website = value;
                    NotifyOfPropertyChange(() => Website);
                }
            }
        }

        [JsonProperty(PropertyName = "Phone")]
        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    NotifyOfPropertyChange(() => Phone);
                }
            }
        }

        [JsonProperty(PropertyName = "Fanpage")]
        private string _fanpage;
        public string Fanpage
        {
            get { return _fanpage; }
            set
            {
                if (_fanpage != value)
                {
                    _fanpage = value;
                    NotifyOfPropertyChange(() => Fanpage);
                }
            }
        }

        [JsonProperty(PropertyName = "Wikipedia")]
        private string _wikipedia;
        public string Wikipedia
        {
            get { return _wikipedia; }
            set
            {
                if (_wikipedia != value)
                {
                    _wikipedia = value;
                    NotifyOfPropertyChange(() => Wikipedia);
                }
            }
        }

        [JsonProperty(PropertyName = "OpeningDays")]
        private string _openingDays;
        public string OpeningDays
        {
            get { return _openingDays; }
            set
            {
                if (_openingDays != value)
                {
                    _openingDays = value;
                    NotifyOfPropertyChange(() => OpeningDays);
                }
            }
        }

        [JsonProperty(PropertyName = "OpeningHours")]
        private string _openingHours;
        public string OpeningHours
        {
            get { return _openingHours; }
            set
            {
                if (_openingHours != value)
                {
                    _openingHours = value;
                    NotifyOfPropertyChange(() => OpeningHours);
                }
            }
        }

        [JsonProperty(PropertyName = "Category")]
        private string _category;
        public string Category
        {
            get { return _category; }
            set
            {
                if (_category != value)
                {
                    _category = value;
                    NotifyOfPropertyChange(() => Category);
                }
            }
        }

        [JsonProperty(PropertyName = "Price")]
        private string _price;
        public string Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    NotifyOfPropertyChange(() => Price);
                }
            }
        }

        [JsonProperty(PropertyName = "Owner")]
        private string _owner;
        public string Owner
        {
            get { return _owner; }
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    NotifyOfPropertyChange(() => Owner);
                }
            }
        }
    }
}
