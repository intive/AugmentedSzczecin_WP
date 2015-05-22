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
using AugmentedSzczecin.Events;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.Views;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.System;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel : Screen, IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent>
    {
        private readonly string _bingKey = "AsaWb7fdBJmcC1YW6uC1UPb57wfLh9cmeX6Zq_r9s0k49tFScWa3o3Z0Sk7ZUo3I";

        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly ILocationService _locationService;
        private readonly IPointOfInterestService _pointOfInterestService;

        public CurrentMapViewModel(IEventAggregator eventAggregator,
            ILocationService locationService, IPointOfInterestService pointOfInterestService,
            INavigationService navigationService)
        {
            _eventAggregator = eventAggregator;
            _navigationService = navigationService;
            _locationService = locationService;
            _pointOfInterestService = pointOfInterestService;
        }

        private ObservableCollection<PointOfInterest> _mapLocations;
        public ObservableCollection<PointOfInterest> MapLocations
        {
            get
            {
                return _mapLocations;
            }
            set
            {
                if (value != _mapLocations)
                {
                    _mapLocations = value;
                    NotifyOfPropertyChange(() => MapLocations);
                }
            }
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

        private bool _geolocationEnabled;
        public bool GeolocationEnabled
        {
            get { return _geolocationEnabled; }
            set
            {
                if (value != _geolocationEnabled)
                {
                    _geolocationEnabled = value;
                    NotifyOfPropertyChange(() => GeolocationEnabled);
                }
            }
        }

        public string BingKey
        {
            get { return _bingKey; }
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

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
            base.OnActivate();

            CountZoomLevel();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            UpdateInternetConnection();
            UpdateGeolocationEnabled();
            CheckConnectionsAvailability();
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        private void CheckConnectionsAvailability()
        {
            if (!InternetConnection && !GeolocationEnabled)
            {
                BothConnectionDisabledMessage();
            }
            if (!InternetConnection && GeolocationEnabled)
            {
                InternetConnectionDisabledMessage();
            }
            if (InternetConnection && !GeolocationEnabled)
            {
                GeolocationDisabledMessage();
            }
            if (!InternetConnection || !GeolocationEnabled) return;

            SetGeolocation();
            _mapLocations = new ObservableCollection<PointOfInterest>();
            RefreshPointOfInterestService();
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

        public void RefreshPointOfInterestService()
        {
            _pointOfInterestService.Refresh();
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

        private void UpdateGeolocationEnabled()
        {
            var isGeolocationEnabled = _locationService.IsGeolocationEnabled();

            if (!isGeolocationEnabled)
            {
                GeolocationEnabled = false;
                return;
            }

            GeolocationEnabled = true;
        }

        public void RefreshConnectionClick()
        {
            UpdateInternetConnection();
            UpdateGeolocationEnabled();
            CheckConnectionsAvailability();
        }

        private void BothConnectionDisabledMessage()
        {
            var loader = new ResourceLoader();
            var bothConnectionDisabledMessage = loader.GetString("BothConnectionDisabledMessage");
            ShowConnectionDisabledMessage(bothConnectionDisabledMessage, "2");
        }

        public void InternetConnectionDisabledMessage()
        {
            var loader = new ResourceLoader();
            var internetConnectionDisabledMessage = loader.GetString("InternetConnectionDisabledMessage");
            ShowConnectionDisabledMessage(internetConnectionDisabledMessage, "1");
        }

        private void GeolocationDisabledMessage()
        {
            var loader = new ResourceLoader();
            var geolocationDisabledMessage = loader.GetString("GeolocationDisabledMessage");
            ShowConnectionDisabledMessage(geolocationDisabledMessage, "0");
        }

        private void ShowConnectionDisabledMessage(string connectionDisabledMessage, object id)
        {
            var message = new MessageDialog(connectionDisabledMessage);
            message.Commands.Add(new UICommand("Back", BackButtonInvokedHandler));
            message.Commands.Add(new UICommand("Settings", BackButtonInvokedHandler, id));
            message.DefaultCommandIndex = 0;
            message.CancelCommandIndex = 1;

            message.ShowAsync();
        }

        public void NavigateToMain()
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            _navigationService.GoBack();
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            MapLocations = e.PointOfInterestList;
        }

        public async void Handle(PointOfInterestLoadFailedEvent e)
        {
            var message = new MessageDialog(e.PointOfInterestLoadException.Message);
            await message.ShowAsync();
        }

        private void BackButtonInvokedHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "Back":
                    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                    NavigateToMain();
                    break;
                case "Settings":
                    if(ReferenceEquals(command.Id, "2"))
                        Launcher.LaunchUriAsync(new Uri("ms-settings-wifi://"));
                    if (ReferenceEquals(command.Id, "1"))
                        Launcher.LaunchUriAsync(new Uri("ms-settings-wifi://"));
                    if (ReferenceEquals(command.Id, "0"))
                        Launcher.LaunchUriAsync(new Uri("ms-settings-location://"));
                    break;
                default:
                    return;
            }
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

        private async void SetGeolocation()
        {
            CenterOfTheMap = await _locationService.GetGeolocation();
        }

        private void CountZoomLevel()
        {
            ZoomLevel = ResolutionHelper.CountZoomLevel();
        }

        private void PushpinTapped(object sender)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
