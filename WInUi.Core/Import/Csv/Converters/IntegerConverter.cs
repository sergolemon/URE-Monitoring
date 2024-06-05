using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace URE.Core.Import.Csv.Converters
{
    public class IntegerConverter : ImportConverter
    {
        public IntegerConverter(Action<IReaderRow> errorHandler): base(errorHandler) { }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            int value = default(int);

            if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out value))
            {
                _errorHandler(row);
            }

            return value;
        }
    }
}
