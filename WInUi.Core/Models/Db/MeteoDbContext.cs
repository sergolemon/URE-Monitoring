using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using URE.Core.Models.Equipment;
using URE.Core.Models.Identity;
using URE.Core.Models.Meteo;

namespace URE.Core.Models.Db
{
    public class MeteoDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<MeteoStream> MeteoStreams { get; set; }

        public DbSet<MeteoData> MeteoData { get; set; }

        public DbSet<GMSettings> GMSettings { get; set; }

        public DbSet<DetectorSettings> DetectorSettings { get; set; }

        public DbSet<GPSSettings> GPSSettings { get; set; }

        public DbSet<MeteoStationSettings> MeteoStationSettings { get; set; }

        public DbSet<UserIdentity> UserIdentity { get; set; }

        public MeteoDbContext(DbContextOptions<MeteoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GPSSettings>().HasData(
                new GPSSettings
                {
                    Id = 1,
                    SerialPortName = string.Empty,
                    BaudRate = 9600,
                    HeightEnabled = true
                }
            );

            modelBuilder.Entity<MeteoStationSettings>().HasData(
                new MeteoStationSettings
                {
                    Id = 1,
                    HumidityEnabled = true,
                    TemperatureEnabled = true,
                    WindDirectionEnabled = true,
                    WindSpeedEnabled = true,
                    PreassureEnabled = true,
                    SerialPortName = string.Empty,
                    BaudRate = 57600
                });

            modelBuilder.Entity<GMSettings>().HasData(
                new GMSettings
                {
                    Id = 1,
                    SerialPortName = string.Empty,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8,
                    BaudRate = 57600,
                    Duration = 60,
                    BlackoutPeriod = 30,
                    RepetitionInterval = 2
                }
            );

            modelBuilder.Entity<DetectorSettings>().HasData(
                new DetectorSettings
                {
                    Id = 1,
                    GmSettingsId = 1,
                    Name = "Detector1",
                    IsEnabled = true,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 1,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8
                },
                new DetectorSettings
                {
                    Id = 2,
                    GmSettingsId = 1,
                    Name = "Detector2",
                    IsEnabled = true,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 3,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8
                },
                new DetectorSettings
                {
                    Id = 3,
                    GmSettingsId = 1,
                    Name = "Detector3",
                    IsEnabled = false,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 255,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8,
                },
                new DetectorSettings
                {
                    Id = 4,
                    GmSettingsId = 1,
                    Name = "Detector4",
                    IsEnabled = false,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 255,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8
                },
                new DetectorSettings
                {
                    Id = 5,
                    GmSettingsId = 1,
                    Name = "Detector5",
                    IsEnabled = false,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 255,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8
                },
                new DetectorSettings
                {
                    Id = 6,
                    GmSettingsId = 1,
                    Name = "Detector6",
                    IsEnabled = false,
                    //BaudRate = 57600,
                    EquipmentIdentifier = 255,
                    SerialNumber = 000003,
                    Color = 255,
                    NormalValue = 0,
                    HighValue = 0.5,
                    CriticalValue = 0.8
                }
            );

            SeedUser(modelBuilder);

            modelBuilder.Entity<MeteoStream>().Property(x => x.UserId).HasDefaultValue(superAdminUserId.ToString());

            base.OnModelCreating(modelBuilder);
        }

        Guid superAdminUserId = Guid.Parse("9807a71f-c6d8-4d2d-8d06-079ebae45450");

        private void SeedUser(ModelBuilder modelBuilder)
        {
            Guid superAdminRoleId = Guid.Parse("0bdaa8f0-aaed-41e7-97b1-d5e7ab5b5531");
            Guid adminRoleId = Guid.Parse("19a20dd4-32f2-4029-bc68-8344ee3fecdf");
            Guid operatorRoleId = Guid.Parse("6d12138c-7929-4dfd-b4e9-a76be12e3ed7");

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = superAdminRoleId.ToString(),
                    Name = Role.SuperAdmin,
                    NormalizedName = Role.SuperAdmin.ToUpper()
                },
                new ApplicationRole
                {
                    Id = adminRoleId.ToString(),
                    Name = Role.Admin,
                    NormalizedName = Role.Admin.ToUpper()
                },
                new ApplicationRole
                {
                    Id = operatorRoleId.ToString(),
                    Name = Role.Operator,
                    NormalizedName = Role.Operator.ToUpper()
                }
            );

            ApplicationUser superAdmin = new ApplicationUser
            {
                Id = superAdminUserId.ToString(),
                Email = "test@test.com",
                EmailConfirmed = true,
                UserName = "SuperAdmin",
                NormalizedUserName = "SUPERADMIN",
                CarNumber = "KA6321KK",
                IsActive = true,
                Name = string.Empty,
                Surname = string.Empty,
                Patronymic = string.Empty
            };

            PasswordHasher<ApplicationUser> hasher = new PasswordHasher<ApplicationUser>();
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "test_password");

            modelBuilder.Entity<ApplicationUser>().HasData(superAdmin);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = superAdminRoleId.ToString(),
                    UserId = superAdminUserId.ToString()
                },
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId.ToString(),
                    UserId = superAdminUserId.ToString()
                },
                new IdentityUserRole<string>
                {
                    RoleId = operatorRoleId.ToString(),
                    UserId = superAdminUserId.ToString()
                }
            );
        }
    }
}
