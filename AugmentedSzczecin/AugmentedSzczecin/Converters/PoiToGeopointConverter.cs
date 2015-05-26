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
            PointOfInterest poi = (PointOfInterest)value;
            if (poi.Location.Latitude > -90 && poi.Location.Latitude < 90 && poi.Location.Longitude > -180 && poi.Location.Longitude < 180)
            {
                return new Geopoint(new BasicGeoposition() { Latitude = poi.Location.Latitude, Longitude = poi.Location.Longitude });
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
