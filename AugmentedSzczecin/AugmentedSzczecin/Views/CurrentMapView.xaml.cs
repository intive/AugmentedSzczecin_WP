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
    public sealed partial class CurrentMapView
    {
        public CurrentMapView()
        {
            InitializeComponent();
        }
    }
}
