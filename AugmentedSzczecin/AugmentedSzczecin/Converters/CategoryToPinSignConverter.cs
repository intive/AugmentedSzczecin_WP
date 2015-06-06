using AugmentedSzczecin.Models;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class CategoryToPinSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            CategoryType cat = (CategoryType)Enum.Parse(typeof(CategoryType), (string)value);
            Symbol symbol = Symbol.Home;
            switch (cat)
            {
                case CategoryType.EVENT:
                    symbol = Symbol.SolidStar;
                    break;
                case CategoryType.PERSON:
                    symbol = Symbol.People;
                    break;
                case CategoryType.COMMERCIAL:
                    symbol = Symbol.Library;
                    break;
            }

            return symbol;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
