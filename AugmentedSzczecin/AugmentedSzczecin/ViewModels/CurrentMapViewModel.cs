using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using System;
using System.Net;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Controls;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel : Screen
    {
        private INavigationService _navigationService;
        private ILocationService _locationService;

        public CurrentMapViewModel(ILocationService locationService, INavigationService navigationService)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            _navigationService = navigationService;
            _locationService = locationService;

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var str = loader.GetString("CurrentMapTitle");

            CountResolution();

            UpdateInternetConnection();
        }

        private bool _internetConnection;

        public bool InternetConnection
        {
            get
            {
                return _internetConnection;
            }
            set
            {
                if (value != _internetConnection)
                {
                    _internetConnection = value;
                    NotifyOfPropertyChange(() => InternetConnection);
                }
            }
        }

        public void UpdateInternetConnection()
        {
            ConnectionProfile internetConnectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();

            if (internetConnectionProfile == null)
            {
                InternetConnection = false;

                return;
            }

            InternetConnection = true;
        }

        public async void InternetConnectionDisabledMsg()
        {
            var msg = new MessageDialog("Internet Connection disabled.");
            msg.Commands.Add(new UICommand("Back", new UICommandInvokedHandler(this.BackButtonInvokedHandler)));
            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 0;

            await msg.ShowAsync();
        }

        protected override void OnActivate()
        {
            if (_locationService.IsGeolocationEnabled())
            {
                SetGeolocation();
            }
            else
            {
                GeolocationDisabledMsg();
            }
        }

        public void NavigateToMain()
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            _navigationService.GoBack();
        }

        private async void GeolocationDisabledMsg()
        {
            var msg = new MessageDialog("Geolocation disabled.");
            msg.Commands.Add(new UICommand("Back", new UICommandInvokedHandler(this.BackButtonInvokedHandler)));
            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 0;

            await msg.ShowAsync();
        }

        private void BackButtonInvokedHandler(IUICommand command)
        {
            if (command.Label == "Back")
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                NavigateToMain();
            }
            else
                return;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }

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

        private void CountResolution()
        {
            float logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            float zoom = 100000 / (logicalDpi * (float)39.37);
            if (zoom > (float)7.17 && zoom < (float)14.33)
                ZoomLevel = 15;
            if (zoom > (float)14.33 && zoom < (float)28.61)
                ZoomLevel = 14;
            if (zoom > (float)28.61 && zoom < (float)57.22)
                ZoomLevel = 13;
            if (zoom > (float)57.22 && zoom < (float)114.44)
                ZoomLevel = 12;
        }

        private double _zoomLevel;

        public double ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            set
            {
                if (_zoomLevel != value)
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
                    ChangeScaleBar(null);
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

        public void ChangeScaleBar(MapControl temporaryMap)
        {
            double tempZoomLevel = ZoomLevel;
            if (temporaryMap != null)
                tempZoomLevel = temporaryMap.ZoomLevel;
            double tempLatitude = 0;
            if (CenterOfTheMap != null)
                tempLatitude = CenterOfTheMap.Position.Latitude;
            tempLatitude = tempLatitude * (Math.PI / 180);

            const double BING_MAP_CONSTANT = 156543.04;

            double metersPerPixel = BING_MAP_CONSTANT * Math.Cos(tempLatitude) / Math.Pow(2, tempZoomLevel);

            double scaleDistance = Math.Round(100 * metersPerPixel / 10) * 10;

            ScaleText = scaleDistance.ToString() + " m";
        }

        private string _scaleText = "";
        public string ScaleText
        {
            get
            {
                return _scaleText;
            }
            set
            {
                if (_scaleText != value)
                {
                    _scaleText = value;
                    NotifyOfPropertyChange(() => ScaleText);
                }
            }
        }
    }
}
