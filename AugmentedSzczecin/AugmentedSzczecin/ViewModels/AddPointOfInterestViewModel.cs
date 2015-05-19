using System;
using Windows.Devices.Geolocation;
using Windows.Networking.Connectivity;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using AugmentedSzczecin.Helpers;
using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class AddPointOfInterestViewModel : Screen
    {
        private readonly string _bingKey = "AsaWb7fdBJmcC1YW6uC1UPb57wfLh9cmeX6Zq_r9s0k49tFScWa3o3Z0Sk7ZUo3I";

        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly ILocationService _locationService;

        public AddPointOfInterestViewModel(IEventAggregator eventAggregator, INavigationService navigationService, ILocationService locationService)
        {
            _eventAggregator = eventAggregator;
            _navigationService = navigationService;
            _locationService = locationService;
        }

        private bool _internetConnection;
        public bool InternetConnection
        {
            get { return _internetConnection; }
            set
            {
                if (value != _internetConnection)
                {
                    _internetConnection = value;
                    NotifyOfPropertyChange(() => InternetConnection);
                }
            }
        }

        public string BingKey
        {
            get { return _bingKey; }
        }

        private readonly bool _landmarksVisible = true;
        public bool LandmarksVisible
        {
            get { return _landmarksVisible; }
        }

        private Geopoint _centerOfTheMap;
        public Geopoint CenterOfTheMap
        {
            get { return _centerOfTheMap; }
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
            get { return _myLocationPointVisibility; }
            set
            {
                if (value != _myLocationPointVisibility)
                {
                    _myLocationPointVisibility = value;
                    NotifyOfPropertyChange(() => MyLocationPointVisibility);
                }
            }
        }

        private string _scaleText = "";
        public string ScaleText
        {
            get { return _scaleText; }
            set
            {
                if (_scaleText != value)
                {
                    _scaleText = value;
                    NotifyOfPropertyChange(() => ScaleText);
                }
            }
        }

        private double _zoomLevel;
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    NotifyOfPropertyChange(() => ZoomLevel);
                }
            }
        }

        public void ChangeScaleBar(MapControl temporaryMap)
        {
            double tempZoomLevel = ZoomLevel;
            if (temporaryMap != null)
            {
                tempZoomLevel = temporaryMap.ZoomLevel;
            }

            double tempLatitude = 0;
            if (CenterOfTheMap != null)
            {
                tempLatitude = CenterOfTheMap.Position.Latitude;
            }

            tempLatitude = tempLatitude * (Math.PI / 180);

            const double bingMapConstant = 156543.04;

            double metersPerPixel = bingMapConstant * Math.Cos(tempLatitude) / Math.Pow(2, tempZoomLevel);

            double scaleDistance = Math.Round(100 * metersPerPixel / 10) * 10;

            ScaleText = scaleDistance.ToString() + " m";
        }

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
            base.OnActivate();

            CountZoomLevel();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (_locationService.IsGeolocationEnabled())
            {
                SetGeolocation();
            }
            else
            {
                GeolocationDisabledMsg();
            }

            UpdateInternetConnection();
            if (!InternetConnection)
            {
                InternetConnectionDisabledMsg();
            }
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        public void UpdateInternetConnection()
        {
            ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

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
            msg.Commands.Add(new UICommand("Back", BackButtonInvokedHandler));
            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 1;

            await msg.ShowAsync();
        }

        private async void SetGeolocation()
        {
            CenterOfTheMap = await _locationService.GetGeolocation();
        }

        private void CountZoomLevel()
        {
            ZoomLevel = ResolutionHelper.CountZoomLevel();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        private async void GeolocationDisabledMsg()
        {
            var msg = new MessageDialog("Geolocation disabled.");
            msg.Commands.Add(new UICommand("Back", BackButtonInvokedHandler));
            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 1;

            await msg.ShowAsync();
        }

        private void BackButtonInvokedHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "Back":
                    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                    NavigateToMain();
                    break;
                default:
                    return;
            }
        }

        public void NavigateToMain()
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            _navigationService.GoBack();
        }
    }
}
