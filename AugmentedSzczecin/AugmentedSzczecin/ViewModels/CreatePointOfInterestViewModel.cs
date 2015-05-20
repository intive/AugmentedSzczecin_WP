using Windows.Devices.Geolocation;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        public Geopoint Parameter { get; set; }

        private string _latitude;
        public string Latitude
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

        private string _longitude;
        public string Longitude
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

        protected override void OnActivate()
        {
            Latitude = Parameter.Position.Latitude.ToString();
            Longitude = Parameter.Position.Longitude.ToString();
        }

    }
}
