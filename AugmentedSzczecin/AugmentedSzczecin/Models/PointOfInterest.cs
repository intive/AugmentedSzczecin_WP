using System;
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

        [JsonProperty(PropertyName = "zipCode")]
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

        //[JsonProperty(PropertyName = "category")]
        //private string _category;
        //public string Category
        //{
        //    get
        //    {
        //        return _category;
        //    }
        //    set
        //    {
        //        if (_category != value)
        //        {
        //            _category = value;
        //            NotifyOfPropertyChange(() => Category);
        //        }
        //    }
        //}

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

        //[JsonProperty(PropertyName = "Price")]
        //private string _price;
        //public string Price
        //{
        //    get
        //    {
        //        return _price;
        //    }
        //    set
        //    {
        //        if (_price != value)
        //        {
        //            _price = value;
        //            NotifyOfPropertyChange(() => Price);
        //        }
        //    }
        //}

        //[JsonProperty(PropertyName = "Owner")]
        //private string _owner;
        //public string Owner
        //{
        //    get
        //    {
        //        return _owner;
        //    }
        //    set
        //    {
        //        if (_owner != value)
        //        {
        //            _owner = value;
        //            NotifyOfPropertyChange(() => Owner);
        //        }
        //    }
        //}
    }
}
