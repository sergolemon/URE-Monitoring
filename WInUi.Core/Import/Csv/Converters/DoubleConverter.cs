using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace URE.Core.Import.Csv.Converters
{
    public class DoubleConverter : ImportConverter
    {
        public DoubleConverter(Action<IReaderRow> errorHandler) : base(errorHandler) { }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            double value = default(double);

            if (!string.IsNullOrEmpty(text) && !double.TryParse(text, CultureInfo.InvariantCulture, out value))
            {
                _errorHandler(row);
            }

            return value;
        }
    }
}
