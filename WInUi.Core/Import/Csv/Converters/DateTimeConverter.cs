using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace URE.Core.Import.Csv.Converters
{
    public class DateTimeConverter : ImportConverter
    {
        private readonly string _format = string.Empty;
        private readonly IFormatProvider _formatProvider = null;
        private readonly DateTimeStyles _style = DateTimeStyles.None;

        public DateTimeConverter(Action<IReaderRow> errorHandler) : base(errorHandler) { }

        public DateTimeConverter(string format,
                                 Action<IReaderRow> errorHandler,
                                 DateTimeStyles style = DateTimeStyles.None,
                                 IFormatProvider formatProvider = null) : base(errorHandler)
        {
            _format = format;
            _style = style;
            _formatProvider = formatProvider;
        }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            DateTime value = default(DateTime);

            if (!string.IsNullOrEmpty(text))
            {
                bool result = string.IsNullOrEmpty(_format) ? DateTime.TryParse(text, out value) :
                    DateTime.TryParseExact(text, _format, _formatProvider, _style, out value);

                if (!result)
                {
                    _errorHandler(row);
                }
            }

            return value;
        }
    }
}
