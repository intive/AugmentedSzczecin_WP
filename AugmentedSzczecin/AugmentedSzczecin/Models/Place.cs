using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Models
{
	public class Place : PropertyChangedBase
	{
		[JsonProperty(PropertyName = "id")]
		public string id { get; set; }

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

		[JsonProperty(PropertyName = "Address")]
		private string _address;
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

		[JsonProperty(PropertyName = "Logitude")]
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

		[JsonProperty(PropertyName = "HasWifi")]
		private bool _hasWifi;
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
