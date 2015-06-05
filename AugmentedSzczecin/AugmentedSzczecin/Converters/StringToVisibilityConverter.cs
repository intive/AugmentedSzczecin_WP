using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string information = (string)value;

            if(String.IsNullOrEmpty(information))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
