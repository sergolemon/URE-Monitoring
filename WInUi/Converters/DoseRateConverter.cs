using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Converters
{
    public class DoseRateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //value from double to string type
            string result = ((double)value).ToString("0.000");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            //value from string to double type
            double result = double.TryParse(((string)value).Replace('.', ','), out var doubleValue) ? Math.Round(doubleValue, 3) : 0.000;

            return result;
        }
    }
}
