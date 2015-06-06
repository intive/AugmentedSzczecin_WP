using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace AugmentedSzczecin.Converters
{
    public class StringToCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string category = (string)value;
            string nameOfCategory = "Miejsca publiczne";
            switch(category)
            {
                case "EVENT":
                    nameOfCategory = "Wydarzenia";
                    break;
                case "COMMERCIAL":
                    nameOfCategory = "Firmy i usługi";
                    break;
                case "PERSON":
                    nameOfCategory = "Znajomi";
                    break;
            }

            return nameOfCategory.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
