using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;

namespace URE.Core.Import.Csv.Converters
{
    public class TimeSpanConverter : ImportConverter
    {
        public TimeSpanConverter(Action<IReaderRow> errorHandler) : base(errorHandler) { }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            TimeSpan value = default(TimeSpan);

            if (!string.IsNullOrEmpty(text))
            {
                bool result = TryParseTimeSpan(text, out value);
                if (!result)
                {
                    _errorHandler(row);
                }
            }

            return value;
        }

        static bool TryParseTimeSpan(string input, out TimeSpan result)
        {
            result = TimeSpan.Zero;

            string[] parts = input.Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], CultureInfo.InvariantCulture, out int hours) ||
                !int.TryParse(parts[1], CultureInfo.InvariantCulture, out int minutes) ||
                !double.TryParse(parts[2], CultureInfo.InvariantCulture, out double seconds))
            {
                return false;
            }

            result = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            return true;
        }
    }
}
