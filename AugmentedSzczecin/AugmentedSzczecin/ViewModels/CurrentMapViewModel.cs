using AugmentedSzczecin.Model;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel :  Screen
    {

        public CurrentMapViewModel()
        {
            //setGeolocation();
            mapLocations = new ObservableCollection<LocationForMap>();
            //showAdditionalLocations();
        }

        protected override void OnActivate()
        {
            setGeolocation();
            //mapLocations = new ObservableCollection<LocationForMap>();
            showAdditionalLocations();
        }


        private void showAdditionalLocations()
        {
            MapLocations.Add(new LocationForMap
            {
                Geopoint = new Geopoint(new BasicGeoposition
                {
                    Longitude = -122.1311156,
                    Latitude = 47.6785619
                }),
                Name = "Redmond"
            });

            MapLocations.Add(new LocationForMap
            {
                Geopoint = new Geopoint(new BasicGeoposition
                {
                    Longitude = -122.1381556,
                    Latitude = 47.6796119
                }),
                Name = "Moja ulubiona cukiernia"
            });

            MapLocations.Add(new LocationForMap
            {
                Geopoint = new Geopoint(new BasicGeoposition
                {
                    Longitude = 14.536886131390929,
                    Latitude = 53.469053423032165
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
                    NotifyOfPropertyChange(() => CenterOfTheMap);
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

        private ObservableCollection<LocationForMap> mapLocations;

        public ObservableCollection<LocationForMap> MapLocations
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
                    NotifyOfPropertyChange(() => MapLocations);
                }
            }
        }
    }
}
