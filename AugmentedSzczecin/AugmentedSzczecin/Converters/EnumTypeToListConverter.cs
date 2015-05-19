using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class EnumTypeToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            var enumType = value as Type;
            if (enumType == null)
                return null;

            var values = Enum.GetValues((Type)value).Cast<Enum>();

            var selectedValues = values.Select(@enum => @enum.ToString()).ToList();

            List<object> checkedValues = new List<object>();

            foreach(var val in selectedValues)
            {
                var newValue = System.Text.RegularExpressions.Regex.Replace(val, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.ECMAScript).Trim();
                checkedValues.Add(newValue);
            }

            return checkedValues;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
