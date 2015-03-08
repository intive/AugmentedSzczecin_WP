using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Model;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel :  Screen
    {

        private ILocationService _locationService;
         
        public CurrentMapViewModel(ILocationService locationService)
        {
            _locationService = locationService;
            
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var str = loader.GetString("CurrentMapTitle");
        }

        protected override void OnActivate()
        {
            SetGeolocation();
            //ShowAdditionalLocations();
        }

        private async void SetGeolocation()
        {
            CenterOfTheMap = await _locationService.SetGeolocation();
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
                    MyLocationPointVisibility = Visibility.Visible;
                    NotifyOfPropertyChange(() => CenterOfTheMap);
                }
            }
        }

        private Visibility _myLocationPointVisibility = Visibility.Collapsed;

        public Visibility MyLocationPointVisibility 
        { 
            get
            {
                return _myLocationPointVisibility;
            }
            set
            {
                if (value != _myLocationPointVisibility)
                {
                    _myLocationPointVisibility = value;
                    NotifyOfPropertyChange(() => MyLocationPointVisibility);
                }
            }
        }

        
    }
}
