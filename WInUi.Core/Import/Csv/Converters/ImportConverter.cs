using System;
using CsvHelper;
using CsvHelper.TypeConversion;

namespace URE.Core.Import.Csv.Converters
{
    public class ImportConverter : DefaultTypeConverter
    {
        protected readonly Action<IReaderRow> _errorHandler;

        public ImportConverter(Action<IReaderRow> errorHandler)
        {
            _errorHandler = errorHandler;
        }
    }
}
