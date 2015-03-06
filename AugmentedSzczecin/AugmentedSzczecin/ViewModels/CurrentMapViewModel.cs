using AugmentedSzczecin.Model;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel :  Screen
    {

        public CurrentMapViewModel()
        {
            _mapLocations = new ObservableCollection<LocationForMap>();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var str = loader.GetString("CurrentMapTitle");
        }

        protected override void OnActivate()
        {
            SetGeolocation();
            ShowAdditionalLocations();
        }

        private void ShowAdditionalLocations()
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

        private string _bingKey = "AsaWb7fdBJmcC1YW6uC1UPb57wfLh9cmeX6Zq_r9s0k49tFScWa3o3Z0Sk7ZUo3I";

        public string BingKey
        {
            get
            {
                return _bingKey;
            }
        }

        private double _zoomLevel = 13;

        public double ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            set
            {
                if(_zoomLevel != value)
                {
                    _zoomLevel = value;
                    NotifyOfPropertyChange(() => ZoomLevel);
                }
            }
        }

        private bool _landmarksVisible = true;

        public bool LandmarksVisible
        {
            get
            {
                return _landmarksVisible;
            }
        }

        private Geopoint _centerOfTheMap;

        public Geopoint CenterOfTheMap
        {
            get
            {
                return _centerOfTheMap;
            }
            set
            {
                if (_centerOfTheMap != value)
                {
                    _centerOfTheMap = value;
                    NotifyOfPropertyChange(() => CenterOfTheMap);
                }
            }
        }

        private async void SetGeolocation()
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

        private ObservableCollection<LocationForMap> _mapLocations;

        public ObservableCollection<LocationForMap> MapLocations
        {
            get
            {
                return _mapLocations;
            }
            set
            {
                if (_mapLocations != value)
                {
                    _mapLocations = value;
                    NotifyOfPropertyChange(() => MapLocations);
                }
            }
        }
    }
}
