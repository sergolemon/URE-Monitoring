using System;
using CsvHelper;
using CsvHelper.Configuration;


namespace URE.Core.Import.Csv.Converters
{
    public class BooleanConverter : ImportConverter
    {
        public BooleanConverter(Action<IReaderRow> errorHandler) : base(errorHandler) { }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            bool value = false;

            if (!string.IsNullOrEmpty(text) && !bool.TryParse(text, out value))
            {
                _errorHandler(row);
            }

            return value;
        }
    }
}
