using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Converters
{
    internal class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Single))
                return float.Parse(((double)value).ToString("0.000"));
            else if (targetType == typeof(string))
                return ((double)value).ToString("0.0");
            else throw new NotImplementedException($"DoubleConverter is not supported {targetType.Name} for convert!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value;
        }
    }
}
