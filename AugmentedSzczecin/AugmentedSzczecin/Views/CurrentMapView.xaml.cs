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
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.ViewModels;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls.Primitives;

namespace AugmentedSzczecin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CurrentMapView : IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent>
    {
        private ObservableCollection<PointOfInterest> _mapLocations;
        readonly object _eventAgg;

        public CurrentMapView()
        {
            InitializeComponent();
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
            {
                ((CurrentMapViewModel)servicesFromCurrentMapViewModel).InternetConnectionDisabledMsg();
            }
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            _mapLocations = e.PointOfInterestList;
            POIs.ItemsSource = _mapLocations;
        }

        public void Handle(PointOfInterestLoadFailedEvent e)
        {
            var msg = new MessageDialog(e.PointOfInterestLoadException.Message);
            msg.ShowAsync();
        }

        private void PushpinTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
