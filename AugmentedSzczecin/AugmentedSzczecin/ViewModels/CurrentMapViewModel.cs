using AugmentedSzczecin.Model;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel : Screen, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public CurrentMapViewModel()
        {
            setGeolocation();
            mapLocations = new List<Location>();
            showAdditionalLocations();
        }

        private void showAdditionalLocations()
        {
            MapLocations.Add(new Location
            {
                Geopoint = new Geopoint(new BasicGeoposition
                {
                    Longitude = -122.1311156,
                    Latitude = 47.6785619
                }),
                Name = "Redmond"
            });

            MapLocations.Add(new Location
            {
                Geopoint = new Geopoint(new BasicGeoposition
                {
                    Longitude = -122.1381556,
                    Latitude = 47.6796119
                }),
                Name = "Moja ulubiona cukiernia"
            });
        }

        private string bingKey = "AsaWb7fdBJmcC1YW6uC1UPb57wfLh9cmeX6Zq_r9s0k49tFScWa3o3Z0Sk7ZUo3I";

        public string BingKey
        {
            get
            {
                return bingKey;
            }
        }

        private double zoomLevel = 15;

        public double ZoomLevel
        {
            get
            {
                return zoomLevel;
            }
        }

        private bool landmarksVisible = true;

        public bool LandmarksVisible
        {
            get
            {
                return landmarksVisible;
            }
        }

        private Geopoint centerOfTheMap;

        public Geopoint CenterOfTheMap
        {
            get
            {
                return centerOfTheMap;
            }
            set
            {
                if (centerOfTheMap != value)
                {
                    centerOfTheMap = value;
                    NotifyPropertyChanged("CenterOfTheMap");
                }
            }
        }

        private async void setGeolocation()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            Geoposition geoposition = await geolocator.GetGeopositionAsync(
            maximumAge: TimeSpan.FromMinutes(5),
            timeout: TimeSpan.FromSeconds(10)
            );

            BasicGeoposition myLocation = new BasicGeoposition
            {
                Longitude = geoposition.Coordinate.Longitude,
                Latitude = geoposition.Coordinate.Latitude
            };

            CenterOfTheMap = new Geopoint(myLocation);
        }

        private List<Location> mapLocations;

        public List<Location> MapLocations
        {
            get
            {
                return mapLocations;
            }
            set
            {
                if (mapLocations != value)
                {
                    mapLocations = value;
                    NotifyPropertyChanged("MapLocations");
                }
            }
        }
    }
}
