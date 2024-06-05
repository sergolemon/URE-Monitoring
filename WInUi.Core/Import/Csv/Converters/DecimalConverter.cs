using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace URE.Core.Import.Csv.Converters
{
    public class DecimalConverter : ImportConverter
    {
        public DecimalConverter(Action<IReaderRow> errorHandler) : base(errorHandler) { }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            decimal value = default(decimal);

            if (!string.IsNullOrEmpty(text) && !decimal.TryParse(text, CultureInfo.InvariantCulture, out value))
            {
                _errorHandler(row);
            }

            return value;
        }
    }
}
