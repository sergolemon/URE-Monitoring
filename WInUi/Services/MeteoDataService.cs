using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Globalization;
using URE.Helpers;
using URE.Contracts.Services;
using URE.Core.Models.Meteo;
using URE.Core.Contracts.Repositories;
using Windows.Storage;
using Windows.System.UserProfile;
using System.Text;
using URE.Core.Import.Csv.Converters;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using URE.Core.Models.Identity;

namespace URE.Services
{
    public class MeteoDataService: IMeteoDataService
    {
        private readonly IAppNotificationService _notificationService;
        private readonly IMeteoStreamRepository _meteoStreamRepository;
        private readonly ILogger<MeteoDataService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly Random random = new Random();
        private (double Lat, double Lon) CurrentLoc = (50.443162, 30.520544);

        public MeteoDataService(IAppNotificationService notificationService,
                                IMeteoStreamRepository meteoStreamRepository,
                                ILogger<MeteoDataService> logger,
                                UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _meteoStreamRepository = meteoStreamRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public MeteoData GetMeteoRow() 
        {
            (double Lat, double Lon) coordinates = GenerateNextCoordinates();
            return new MeteoData
            {
                Direction = random.NextDouble(),
                Speed = Math.Round(random.NextDouble() * 100),
                CorrectedDirection = random.Next(360),
                CorrectedSpeed = Math.Round(random.NextDouble() * 50),
                Pressure = random.NextDouble(),
                RelativeHumidity = Math.Round(random.NextDouble() * 100),
                Temperature = Math.Round(random.NextDouble() * 60),
                DewPoint = random.NextDouble(),
                GPSLatitude = coordinates.Lat,
                GPSLongitude = coordinates.Lon,
                GPSHeight = Math.Round(random.NextDouble() * 100),
                Date = DateTime.Now.Date,
                Time = DateTime.Now.TimeOfDay,
                SupplyVoltage = random.NextDouble(),
                D1Radiation = random.NextDouble(),
                D2Radiation = random.NextDouble(),
                D3Radiation = random.NextDouble(),
                D4Radiation = random.NextDouble(),
                D5Radiation = random.NextDouble(),
                D6Radiation = random.NextDouble()
            };    
        }

        public void Export(string path, List<MeteoStream> streams)
        {
            _notificationService.Show("ExportStarted".GetLocalized());

            try
            {
                CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };

                foreach (var stream in streams)
                {
                    using var writer = new StreamWriter($"{path}/URE-{stream.DateStart.ToString("yyyyMMdd_HHmmss")}-{stream.User.PersonInfo}.csv");
                    using var csv = new CsvWriter(writer, csvConfiguration);

                    csv.WriteRecord(new MeteoStreamMetadata
                    {
                        Date = stream.DateStart,
                        Auto = stream.Auto,
                        CarNumber = stream.User.CarNumber,
                        Name = stream.User.Name,
                        Surname = stream.User.Surname,
                        Patronymic = stream.User.Patronymic,
                        Login = stream.User.UserName
                    });

                    csv.NextRecord();
                    foreach (MeteoData row in stream.Data)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }

                _notificationService.Show("ExportEnded".GetLocalized());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export error");
                _notificationService.Show("ExportFailed".GetLocalized());
            }
        }

        public async Task<string> Import(IReadOnlyList<StorageFile> files, ICollection<MeteoStream> outputStreams = null!)
        {
            if (outputStreams != null) outputStreams.Clear();
            StringBuilder protocol = new StringBuilder();

            _notificationService.Show("ImportStarted".GetLocalized());

            try
            {
                foreach (var file in files)
                {
                    using Stream stream = await file.OpenStreamForReadAsync();
                    if (stream.Length == 0)
                    {
                        protocol.AppendLine(String.Format("FileEmpty".GetLocalized(), file.Name));
                        continue;
                    }

                    CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";"
                    };

                    using var reader = new StreamReader(stream);
                    using var csv = new CsvReader(reader, csvConfiguration);

                    protocol.AppendLine(String.Format("FileImportStarted".GetLocalized(), file.Name));

                    InitConverters(csv, protocol);

                    csv.Read();
                    if (!GetMetadata(csv, out MeteoStreamMetadata metadata))
                    {
                        protocol.AppendLine("IncorrectFileFormat".GetLocalized());
                        continue;
                    }

                    List<MeteoData> records = new List<MeteoData>();

                    while (csv.Read())
                    {
                        records.Add(csv.GetRecord<MeteoData>());
                    }

                    if (records.Any())
                    {
                        MeteoStream meteoStream = new MeteoStream
                        {
                            DateStart = metadata.Date,
                            Auto = metadata.Auto
                        };

                        var user = await AssignUser(meteoStream, metadata);

                        MeteoData? lastRecord = records.LastOrDefault();
                        if (lastRecord != null && lastRecord.Date != DateTime.MinValue)
                        {
                            meteoStream.DateEnd = lastRecord.Date + lastRecord.Time;
                        }
                        else
                        {
                            meteoStream.DateEnd = DateTime.Now;
                        }

                        await _meteoStreamRepository.AddMeteoStreamAsync(meteoStream, records);

                        if (outputStreams != null)
                        {
                            meteoStream.Data = records;
                            meteoStream.User = user;
                            outputStreams.Add(meteoStream);
                        }

                        protocol.AppendLine(String.Format("FileImportEnded".GetLocalized(), file.Name))
                                .AppendLine();
                    }
                    else
                    {
                        protocol.AppendLine(String.Format("RouteEmpty".GetLocalized(), file.Name));
                    }
                }

            }
            catch (Exception ex)
            {
                protocol.AppendLine("ImportError".GetLocalized());
                _logger.LogError(ex, "MeteoData import failed");
            }

            return protocol.ToString();
        }

        private void InitConverters(CsvReader reader, StringBuilder protocol)
        {
            Action<IReaderRow> errorHandler = (row) => protocol.AppendLine(FormatImportProtocolInfo(row.Context.Parser.Row, row.CurrentIndex, "IncorrectDataType".GetLocalized()));

            reader.Context.TypeConverterCache.AddConverter<bool>(new BooleanConverter(errorHandler));
            reader.Context.TypeConverterCache.AddConverter<int>(new IntegerConverter(errorHandler));
            reader.Context.TypeConverterCache.AddConverter<decimal>(new DecimalConverter(errorHandler));
            reader.Context.TypeConverterCache.AddConverter<double>(new DoubleConverter(errorHandler));
            reader.Context.TypeConverterCache.AddConverter<DateTime>(new DateTimeConverter("MM/dd/yyyy HH:mm:ss", errorHandler));
            reader.Context.TypeConverterCache.AddConverter<TimeSpan>(new TimeSpanConverter(errorHandler));
        }

        private string FormatImportProtocolInfo(int rowIndex, int columnIndex, string message)
        {
            return $"{"Row".GetLocalized()}: {rowIndex}; {"Column".GetLocalized()}: {columnIndex}; {message}.";
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private (double Lat, double Lon) GenerateNextCoordinates()
        {
            CurrentLoc = (CurrentLoc.Lat + 0.000100, CurrentLoc.Lon + 0.000030);
            return CurrentLoc;
        }

        private bool GetMetadata(CsvReader csv, out MeteoStreamMetadata metadata)
        {
            bool available = false;
            metadata = new MeteoStreamMetadata
            {
                Date = csv.GetField<DateTime>(0),
                Auto = csv.GetField<bool>(1),
                CarNumber = csv.GetField<string>(2),
                Name = csv.GetField<string>(3),
                Surname = csv.GetField<string>(4),
                Patronymic = csv.GetField<string>(5),
                Login = csv.GetField<string>(6)
            };

            if (!string.IsNullOrEmpty(metadata.CarNumber) && !string.IsNullOrEmpty(metadata.Login))
            {
                available = true;
            }

            return available;
        }

        private async Task<ApplicationUser> AssignUser(MeteoStream stream, MeteoStreamMetadata metadata)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(metadata.Login);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    CarNumber = metadata.CarNumber,
                    Name = metadata.Name,
                    Surname = metadata.Surname,
                    Patronymic = metadata.Patronymic,
                    UserName = metadata.Login,
                    IsActive = false
                };

                var hasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = hasher.HashPassword(user, string.Empty);

                await _userManager.CreateAsync(user);
            }

            stream.UserId = user.Id;

            return user;
        }
    }
}
