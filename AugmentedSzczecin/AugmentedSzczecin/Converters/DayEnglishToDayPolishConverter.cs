using System;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class DayEnglishToDayPolishConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string day = value as string;
            string result = "Poniedziałek";
            switch(day)
            {
                case "TUESDAY":
                    result = "Wtorek";
                    break;
                case "WEDNESDAY":
                    result = "Środa";
                    break;
                case "THURSDAY":
                    result = "Czwartek";
                    break;
                case "FRIDAY":
                    result = "Piątek";
                    break;
                case "SATURDAY":
                    result = "Sobota";
                    break;
                case "SUNDAY":
                    result = "Niedziela";
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
