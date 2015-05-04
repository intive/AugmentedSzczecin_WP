using AugmentedSzczecin.Models;
using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class PoiToGeopointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            PointOfInterest poi = (PointOfInterest) value;
            return new Geopoint(new BasicGeoposition() {Latitude = poi.Latitude, Longitude = poi.Longitude});
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}