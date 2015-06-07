using Caliburn.Micro;
using AugmentedSzczecin.Models;
using System.Collections.Generic;
using AugmentedSzczecin.Interfaces;
using Windows.ApplicationModel.Resources;
using Windows.Phone.UI.Input;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.System;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.AbstractClasses
{
    public abstract class FilteredPOIViewBase : Screen
    {
        protected readonly IEventAggregator _eventAggregator;
        protected readonly INavigationService _navigationService;
        protected readonly ILocationService _locationService;
        protected readonly IPointOfInterestService _pointOfInterestService;

        public FilteredPOIViewBase(IEventAggregator eventAggregator,
                                    ILocationService locationService,
                                    IPointOfInterestService pointOfInterestService,
                                    INavigationService navigationService)
        {
            _eventAggregator = eventAggregator;
            _navigationService = navigationService;
            _locationService = locationService;
            _pointOfInterestService = pointOfInterestService;
        }

        private List<Category> _listOfCategories = new List<Category>()
                                                    {
                                                        new Category() {Text = "Miejsca publiczne", EnumCategory = CategoryType.PLACE},
                                                        new Category() {Text = "Firmy i usługi", EnumCategory = CategoryType.COMMERCIAL},
                                                        new Category() {Text = "Wydarzenia", EnumCategory = CategoryType.EVENT},
                                                        new Category() {Text = "Znajomi", EnumCategory = CategoryType.PERSON},
                                                        new Category() {Text = "Wszystkie", EnumCategory = CategoryType.ALL},
                                                    };

        public List<Category> ListOfCategories
        {
            get
            {
                return _listOfCategories;
            }
            set
            {
                if (value != _listOfCategories)
                {
                    _listOfCategories = value;
                    NotifyOfPropertyChange(() => ListOfCategories);
                }
            }
        }

        private CategoryType _selectedValue;
        public CategoryType SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                if (value != _selectedValue)
                {
                    _selectedValue = value;
                    FilterByCategory();
                    NotifyOfPropertyChange(() => SelectedValue);
                }
            }
        }

        private bool _isFilterPanelVisible = false;
        public bool IsFilterPanelVisible
        {
            get
            {
                return _isFilterPanelVisible;
            }
            set
            {
                if (value != _isFilterPanelVisible)
                {
                    _isFilterPanelVisible = value;
                    NotifyOfPropertyChange(() => IsFilterPanelVisible);
                }
            }
        }

        protected void FilterByCategory()
        {
            IsFilterPanelVisible = false;
            RefreshPOIFilteredByCategory();
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

        private int _radius = 500;
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                NotifyOfPropertyChange(() => Radius);
                RefreshPOIFilteredByCategory();
            }
        }

        private bool _isInformationPanelVisible = false;
        public bool IsInformationPanelVisible
        {
            get
            {
                return _isInformationPanelVisible;
            }
            set
            {
                _isInformationPanelVisible = value;
                NotifyOfPropertyChange(() => IsInformationPanelVisible);
            }
        }

        private PointOfInterest _pointToShowInformation;
        public PointOfInterest PointToShowInformation
        {
            get
            {
                return _pointToShowInformation;
            }
            set
            {
                _pointToShowInformation = value;
                NotifyOfPropertyChange(() => PointToShowInformation);
            }
        }

        protected override void OnActivate()
        {
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

        public void RefreshConnectionClick()
        {
            UpdateInternetConnection();
            UpdateGeolocationEnabled();
            CheckConnectionsAvailability();
        }

        public void InternetConnectionDisabledMessage()
        {
            var loader = new ResourceLoader();
            var internetConnectionDisabledMessage = loader.GetString("InternetConnectionDisabledMessage");
            ShowConnectionDisabledMessage(internetConnectionDisabledMessage, "1");
        }

        public void NavigateToMain()
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            _navigationService.GoBack();
        }

        public void CheckConnectionsAvailability()
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

            RefreshPOIFilteredByCategory();
        }

        public void UpdateGeolocationEnabled()
        {
            var isGeolocationEnabled = _locationService.IsGeolocationEnabled();

            if (!isGeolocationEnabled)
            {
                GeolocationEnabled = false;
                return;
            }

            GeolocationEnabled = true;
        }

        public void BothConnectionDisabledMessage()
        {
            var loader = new ResourceLoader();
            var bothConnectionDisabledMessage = loader.GetString("BothConnectionDisabledMessage");
            ShowConnectionDisabledMessage(bothConnectionDisabledMessage, "2");
        }

        public void GeolocationDisabledMessage()
        {
            var loader = new ResourceLoader();
            var geolocationDisabledMessage = loader.GetString("GeolocationDisabledMessage");
            ShowConnectionDisabledMessage(geolocationDisabledMessage, "0");
        }

        public void ShowConnectionDisabledMessage(string connectionDisabledMessage, object id)
        {
            var message = new MessageDialog(connectionDisabledMessage);
            message.Commands.Add(new UICommand("Back", BackButtonInvokedHandler));
            message.Commands.Add(new UICommand("Settings", BackButtonInvokedHandler, id));
            message.DefaultCommandIndex = 0;
            message.CancelCommandIndex = 1;

            message.ShowAsync();
        }

        public void BackButtonInvokedHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "Back":
                    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                    NavigateToMain();
                    break;
                case "Settings":
                    if (ReferenceEquals(command.Id, "2"))
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

        public void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        public void CloseInformationPanel()
        {
            IsInformationPanelVisible = false;
        }

        public void PushpinTapped(object sender)
        {
            IsInformationPanelVisible = true;
            PointToShowInformation = (PointOfInterest)sender;
        }

        public void ToggleFilter()
        {
            IsFilterPanelVisible = !IsFilterPanelVisible;
        }

        public async void RefreshPOIFilteredByCategory()
        {
            Geopoint point = await _locationService.GetGeolocation();
            _pointOfInterestService.LoadPoIs(point.Position.Latitude, point.Position.Longitude, Radius, SelectedValue);
        }
    }
}
