using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Data;
using AugmentedSzczecin.Models;

namespace AugmentedSzczecin.Converters
{
    public class PoiToGeopointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            PointOfInterest poi = (PointOfInterest)value;
            if (poi.Location != null)
            {
                if (poi.Location.Latitude > -90 && poi.Location.Latitude < 90 && poi.Location.Longitude > -180 && poi.Location.Longitude < 180)
                {
                    return new Geopoint(new BasicGeoposition() { Latitude = poi.Location.Latitude, Longitude = poi.Location.Longitude });
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
