using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Models
{
	public class PointOfInterest : PropertyChangedBase
	{
		public PointOfInterest(string id)
		{
			this.Id = id;
		}
		public string Id { get; private set; }

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
