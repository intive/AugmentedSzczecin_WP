using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Models
{
	public class Place : PropertyChangedBase
	{
		public string Id { get; set; }

		string _name;
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

		string _address;
		public string Address
		{
			get { return _address; }
			set
			{
				if (_address != value)
				{
					_address = value;
					NotifyOfPropertyChange(() => Address);
				}
			}
		}

		double _latitude;
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

		double _longitude;
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

		bool _hasWifi;
		public bool HasWifi
		{
			get { return _hasWifi; }
			set
			{
				if (_hasWifi != value)
				{
					_hasWifi = value;
					NotifyOfPropertyChange(() => HasWifi);
				}
			}
		}
	}
}
