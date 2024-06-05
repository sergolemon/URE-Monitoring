using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Converters
{
    public class PositiveNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = ((int)value).ToString();

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int result = int.TryParse((string)value, out var intValue) ? intValue : 0;

            return result;
        }
    }
}
