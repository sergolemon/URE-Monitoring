﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using URE.Core.Models.Db;

#nullable disable

namespace URE.Core.Migrations
{
    [DbContext(typeof(MeteoDbContext))]
    [Migration("20240227160434_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityRole");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                            RoleId = "0bdaa8f0-aaed-41e7-97b1-d5e7ab5b5531"
                        },
                        new
                        {
                            UserId = "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                            RoleId = "19a20dd4-32f2-4029-bc68-8344ee3fecdf"
                        },
                        new
                        {
                            UserId = "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                            RoleId = "6d12138c-7929-4dfd-b4e9-a76be12e3ed7"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.DetectorSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Color")
                        .HasColumnType("int");

                    b.Property<double>("CriticalValue")
                        .HasColumnType("float(32)");

                    b.Property<int>("EquipmentIdentifier")
                        .HasColumnType("int");

                    b.Property<int>("GmSettingsId")
                        .HasColumnType("int");

                    b.Property<double>("HighValue")
                        .HasColumnType("float(32)");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)");

                    b.Property<double>("NormalValue")
                        .HasColumnType("float(32)");

                    b.Property<long>("SerialNumber")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("GmSettingsId");

                    b.ToTable("DetectorSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 1,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = true,
                            Name = "Detector1",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        },
                        new
                        {
                            Id = 2,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 3,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = true,
                            Name = "Detector2",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        },
                        new
                        {
                            Id = 3,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 255,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = false,
                            Name = "Detector3",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        },
                        new
                        {
                            Id = 4,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 255,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = false,
                            Name = "Detector4",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        },
                        new
                        {
                            Id = 5,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 255,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = false,
                            Name = "Detector5",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        },
                        new
                        {
                            Id = 6,
                            Color = 255,
                            CriticalValue = 0.80000000000000004,
                            EquipmentIdentifier = 255,
                            GmSettingsId = 1,
                            HighValue = 0.5,
                            IsEnabled = false,
                            Name = "Detector6",
                            NormalValue = 0.0,
                            SerialNumber = 3L
                        });
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.GMSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BaudRate")
                        .HasColumnType("int");

                    b.Property<int>("BlackoutPeriod")
                        .HasColumnType("int");

                    b.Property<double>("CriticalValue")
                        .HasColumnType("float(32)");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<double>("HighValue")
                        .HasColumnType("float(32)");

                    b.Property<double>("NormalValue")
                        .HasColumnType("float(32)");

                    b.Property<int>("RepetitionInterval")
                        .HasColumnType("int");

                    b.Property<string>("SerialPortName")
                        .IsRequired()
                        .HasColumnType("varchar(6)");

                    b.HasKey("Id");

                    b.ToTable("GMSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            BaudRate = 57600,
                            BlackoutPeriod = 30,
                            CriticalValue = 0.80000000000000004,
                            Duration = 60,
                            HighValue = 0.5,
                            NormalValue = 0.0,
                            RepetitionInterval = 2,
                            SerialPortName = "COM6"
                        });
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.GPSSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BaudRate")
                        .HasColumnType("int");

                    b.Property<bool>("HeightEnabled")
                        .HasColumnType("bit");

                    b.Property<double>("MaxMoveSpeed")
                        .HasColumnType("float");

                    b.Property<double>("MinMoveSpeed")
                        .HasColumnType("float");

                    b.Property<int>("MoveSpeedColor")
                        .HasColumnType("int");

                    b.Property<bool>("MoveSpeedEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("SerialPortName")
                        .IsRequired()
                        .HasColumnType("varchar(6)");

                    b.HasKey("Id");

                    b.ToTable("GPSSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            BaudRate = 9600,
                            HeightEnabled = true,
                            MaxMoveSpeed = 0.0,
                            MinMoveSpeed = 0.0,
                            MoveSpeedColor = 0,
                            MoveSpeedEnabled = false,
                            SerialPortName = "COM5"
                        });
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.MeteoStationSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BaudRate")
                        .HasColumnType("int");

                    b.Property<bool>("HumidityEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("PreassureEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("SerialPortName")
                        .IsRequired()
                        .HasColumnType("varchar(6)");

                    b.Property<bool>("TemperatureEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("WindDirectionEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("WindSpeedEnabled")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("MeteoStationSettings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            BaudRate = 57600,
                            HumidityEnabled = true,
                            PreassureEnabled = true,
                            SerialPortName = "COM8",
                            TemperatureEnabled = true,
                            WindDirectionEnabled = true,
                            WindSpeedEnabled = true
                        });
                });

            modelBuilder.Entity("URE.Core.Models.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("CarNumber")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Patronymic")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                            AccessFailedCount = 0,
                            CarNumber = "KA6321KK",
                            ConcurrencyStamp = "40ed038c-0319-489e-8f33-ba40512eff69",
                            Email = "test@test.com",
                            EmailConfirmed = true,
                            IsActive = true,
                            LockoutEnabled = false,
                            Name = "",
                            NormalizedUserName = "SUPERADMIN",
                            PasswordHash = "AQAAAAIAAYagAAAAEJ9IIXyLXjUKaWhWFetMxXKZSRZZF7g+Td9WPM8JPPYKlisg7bEh2RzThCw3hV28UA==",
                            Patronymic = "",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "2298f41b-370f-41de-8da8-f8a128f6641a",
                            Surname = "",
                            TwoFactorEnabled = false,
                            UserName = "SuperAdmin"
                        });
                });

            modelBuilder.Entity("URE.Core.Models.Identity.UserIdentity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserIdentity");
                });

            modelBuilder.Entity("URE.Core.Models.Meteo.MeteoData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<double>("CorrectedDirection")
                        .HasColumnType("float(32)");

                    b.Property<double>("CorrectedSpeed")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D1Radiation")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D2Radiation")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D3Radiation")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D4Radiation")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D5Radiation")
                        .HasColumnType("float(32)");

                    b.Property<double?>("D6Radiation")
                        .HasColumnType("float(32)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<double>("DewPoint")
                        .HasColumnType("float(32)");

                    b.Property<double>("Direction")
                        .HasColumnType("float(32)");

                    b.Property<double>("GPSHeight")
                        .HasColumnType("float");

                    b.Property<double>("GPSLatitude")
                        .HasColumnType("float(32)");

                    b.Property<double>("GPSLongitude")
                        .HasColumnType("float(32)");

                    b.Property<double>("ManualInputRadiation")
                        .HasColumnType("float(32)");

                    b.Property<int>("MeteoStreamId")
                        .HasColumnType("int");

                    b.Property<double>("Pressure")
                        .HasColumnType("float(32)");

                    b.Property<double>("RelativeHumidity")
                        .HasColumnType("float(32)");

                    b.Property<double>("Speed")
                        .HasColumnType("float(32)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<double>("SupplyVoltage")
                        .HasColumnType("float");

                    b.Property<double>("Temperature")
                        .HasColumnType("float(32)");

                    b.Property<TimeSpan>("Time")
                        .HasColumnType("time");

                    b.HasKey("Id");

                    b.HasIndex("Date");

                    b.HasIndex("MeteoStreamId");

                    b.ToTable("MeteoData");
                });

            modelBuilder.Entity("URE.Core.Models.Meteo.MeteoStream", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Auto")
                        .HasColumnType("bit");

                    b.Property<DateTime>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)")
                        .HasDefaultValue("9807a71f-c6d8-4d2d-8d06-079ebae45450");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("MeteoStreams");
                });

            modelBuilder.Entity("URE.Core.Models.Identity.ApplicationRole", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityRole");

                    b.HasDiscriminator().HasValue("ApplicationRole");

                    b.HasData(
                        new
                        {
                            Id = "0bdaa8f0-aaed-41e7-97b1-d5e7ab5b5531",
                            Name = "SuperAdmin",
                            NormalizedName = "SUPERADMIN"
                        },
                        new
                        {
                            Id = "19a20dd4-32f2-4029-bc68-8344ee3fecdf",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "6d12138c-7929-4dfd-b4e9-a76be12e3ed7",
                            Name = "Operator",
                            NormalizedName = "OPERATOR"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.DetectorSettings", b =>
                {
                    b.HasOne("URE.Core.Models.Equipment.GMSettings", null)
                        .WithMany("DetectorSettings")
                        .HasForeignKey("GmSettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("URE.Core.Models.Identity.UserIdentity", b =>
                {
                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URE.Core.Models.Meteo.MeteoData", b =>
                {
                    b.HasOne("URE.Core.Models.Meteo.MeteoStream", null)
                        .WithMany("Data")
                        .HasForeignKey("MeteoStreamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("URE.Core.Models.Meteo.MeteoStream", b =>
                {
                    b.HasOne("URE.Core.Models.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URE.Core.Models.Equipment.GMSettings", b =>
                {
                    b.Navigation("DetectorSettings");
                });

            modelBuilder.Entity("URE.Core.Models.Meteo.MeteoStream", b =>
                {
                    b.Navigation("Data");
                });
#pragma warning restore 612, 618
        }
    }
}
