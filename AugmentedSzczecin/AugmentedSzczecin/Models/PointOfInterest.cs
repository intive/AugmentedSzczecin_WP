using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Models
{
    public class PointOfInterest : PropertyChangedBase
    {
        [JsonProperty(PropertyName = "name")]
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        [JsonProperty(PropertyName = "description")]
        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    NotifyOfPropertyChange(() => Description);
                }
            }
        }

        [JsonProperty(PropertyName = "location")]
        private Location _location;
        public Location Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    NotifyOfPropertyChange(() => Location);
                }
            }
        }

        [JsonProperty(PropertyName = "address")]
        private Address _address;
        public Address Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address != value)
                {
                    _address = value;
                    NotifyOfPropertyChange(() => Address);
                }
            }
        }

        [JsonProperty(PropertyName = "tags")]
        private string[] _tags;
        public string[] Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    NotifyOfPropertyChange(() => Tags);
                }
            }
        }

        [JsonProperty(PropertyName = "www")]
        private string _www;
        public string Www
        {
            get
            {
                return _www;
            }
            set
            {
                if (_www != value)
                {
                    _www = value;
                    NotifyOfPropertyChange(() => Www);
                }
            }
        }

        [JsonProperty(PropertyName = "phone")]
        private string _phone;
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    NotifyOfPropertyChange(() => Phone);
                }
            }
        }

        [JsonProperty(PropertyName = "fanpage")]
        private string _fanpage;
        public string Fanpage
        {
            get
            {
                return _fanpage;
            }
            set
            {
                if (_fanpage != value)
                {
                    _fanpage = value;
                    NotifyOfPropertyChange(() => Fanpage);
                }
            }
        }

        [JsonProperty(PropertyName = "media")]
        private string[] _media;
        public string[] Media
        {
            get
            {
                return _media;
            }
            set
            {
                if (_media != value)
                {
                    _media = value;
                    NotifyOfPropertyChange(() => Media);
                }
            }
        }

        [JsonProperty(PropertyName = "opening")]
        private Opening[] _opening;
        public Opening[] Opening
        {
            get
            {
                return _opening;
            }
            set
            {
                if (_opening != value)
                {
                    _opening = value;
                    NotifyOfPropertyChange(() => Opening);
                }
            }
        }

        [JsonProperty(PropertyName = "category")]
        private string _category;
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (_category != value)
                {
                    _category = value;
                    NotifyOfPropertyChange(() => Category);
                }
            }
        }

        [JsonProperty(PropertyName = "subcategory")]
        private string _subcategory;
        public string Subcategory
        {
            get
            {
                return _subcategory;
            }
            set
            {
                if (_subcategory != value)
                {
                    _subcategory = value;
                    NotifyOfPropertyChange(() => Subcategory);
                }
            }
        }
    }
}