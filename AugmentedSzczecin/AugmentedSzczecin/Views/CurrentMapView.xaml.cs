using System;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.ViewModels;
using Caliburn.Micro;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AugmentedSzczecin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CurrentMapView : Page, IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent>
    {

        private ObservableCollection<PointOfInterest> _mapLocations;
        readonly object _eventAgg;

        public CurrentMapView()
        {
            this.InitializeComponent();
            _mapLocations = new ObservableCollection<PointOfInterest>();

            _eventAgg = IoC.GetInstance(typeof(IEventAggregator), null);
            ((EventAggregator)_eventAgg).Subscribe(this);

            CheckInternetConnection();
        }

        ~CurrentMapView()
        {
            ((EventAggregator)_eventAgg).Unsubscribe(this);
        }

        private void CheckInternetConnection()
        {
            var servicesFromCurrentMapViewModel = IoC.GetInstance(typeof(CurrentMapViewModel), null);
            ((CurrentMapViewModel)servicesFromCurrentMapViewModel).UpdateInternetConnection();
            bool internetConnection = ((CurrentMapViewModel)servicesFromCurrentMapViewModel).InternetConnection;

            if (!internetConnection)
                ((CurrentMapViewModel)servicesFromCurrentMapViewModel).InternetConnectionDisabledMsg();
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            _mapLocations = e.PointOfInterestList;

            if (_mapLocations != null)
                foreach (PointOfInterest pointOfInterest in _mapLocations)
                {
                    var newPin = CreatePin();
                    BingMap.Children.Add(newPin);
                    Geopoint geopoint = new Geopoint(new BasicGeoposition
                            {
                                Longitude = pointOfInterest.Longitude,
                                Latitude = pointOfInterest.Latitude
                            });
                    MapControl.SetLocation(newPin, geopoint);
                    MapControl.SetNormalizedAnchorPoint(newPin, new Point(0.5, 1));
                }
        }

        private DependencyObject CreatePin()
        {
            var myGrid = new Grid();
            myGrid.RowDefinitions.Add(new RowDefinition());
            myGrid.RowDefinitions.Add(new RowDefinition());
            myGrid.Background = new SolidColorBrush(Colors.Transparent);

            Uri uri = new Uri("ms-appx:///Assets/Locationpoint.png", UriKind.Absolute);

            var image = new Image()
            {
                Source = new BitmapImage(uri),
                Width = 10,
                Height = 10
            };

            myGrid.Children.Add(image);

            return myGrid;
        }

        public void Handle(PointOfInterestLoadFailedEvent e)
        {
            var msg = new MessageDialog(e.PointOfInterestLoadException.Message);
            msg.ShowAsync();
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
