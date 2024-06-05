using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace URE.Core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CarNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Patronymic = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GMSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialPortName = table.Column<string>(type: "varchar(6)", nullable: false),
                    BaudRate = table.Column<int>(type: "int", nullable: false),
                    NormalValue = table.Column<double>(type: "float(32)", nullable: false),
                    HighValue = table.Column<double>(type: "float(32)", nullable: false),
                    CriticalValue = table.Column<double>(type: "float(32)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    BlackoutPeriod = table.Column<int>(type: "int", nullable: false),
                    RepetitionInterval = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GMSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GPSSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialPortName = table.Column<string>(type: "varchar(6)", nullable: false),
                    BaudRate = table.Column<int>(type: "int", nullable: false),
                    MoveSpeedEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinMoveSpeed = table.Column<double>(type: "float", nullable: false),
                    MaxMoveSpeed = table.Column<double>(type: "float", nullable: false),
                    MoveSpeedColor = table.Column<int>(type: "int", nullable: false),
                    HeightEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPSSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeteoStationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialPortName = table.Column<string>(type: "varchar(6)", nullable: false),
                    BaudRate = table.Column<int>(type: "int", nullable: false),
                    TemperatureEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HumidityEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WindDirectionEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WindSpeedEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PreassureEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeteoStationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeteoStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "9807a71f-c6d8-4d2d-8d06-079ebae45450"),
                    Auto = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeteoStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeteoStreams_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserIdentity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIdentity_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectorSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GmSettingsId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EquipmentIdentifier = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<long>(type: "bigint", nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false),
                    NormalValue = table.Column<double>(type: "float(32)", nullable: false),
                    HighValue = table.Column<double>(type: "float(32)", nullable: false),
                    CriticalValue = table.Column<double>(type: "float(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectorSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectorSettings_GMSettings_GmSettingsId",
                        column: x => x.GmSettingsId,
                        principalTable: "GMSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeteoData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeteoStreamId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<double>(type: "float(32)", nullable: false),
                    Speed = table.Column<double>(type: "float(32)", nullable: false),
                    CorrectedDirection = table.Column<double>(type: "float(32)", nullable: false),
                    CorrectedSpeed = table.Column<double>(type: "float(32)", nullable: false),
                    Pressure = table.Column<double>(type: "float(32)", nullable: false),
                    RelativeHumidity = table.Column<double>(type: "float(32)", nullable: false),
                    Temperature = table.Column<double>(type: "float(32)", nullable: false),
                    DewPoint = table.Column<double>(type: "float(32)", nullable: false),
                    GPSLatitude = table.Column<double>(type: "float(32)", nullable: false),
                    GPSLongitude = table.Column<double>(type: "float(32)", nullable: false),
                    GPSHeight = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    SupplyVoltage = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    D1Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    D2Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    D3Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    D4Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    D5Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    D6Radiation = table.Column<double>(type: "float(32)", nullable: true),
                    ManualInputRadiation = table.Column<double>(type: "float(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeteoData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeteoData_MeteoStreams_MeteoStreamId",
                        column: x => x.MeteoStreamId,
                        principalTable: "MeteoStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0bdaa8f0-aaed-41e7-97b1-d5e7ab5b5531", null, "ApplicationRole", "SuperAdmin", "SUPERADMIN" },
                    { "19a20dd4-32f2-4029-bc68-8344ee3fecdf", null, "ApplicationRole", "Admin", "ADMIN" },
                    { "6d12138c-7929-4dfd-b4e9-a76be12e3ed7", null, "ApplicationRole", "Operator", "OPERATOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "CarNumber", "ConcurrencyStamp", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "Patronymic", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Surname", "TwoFactorEnabled", "UserName" },
                values: new object[] { "9807a71f-c6d8-4d2d-8d06-079ebae45450", 0, "KA6321KK", "40ed038c-0319-489e-8f33-ba40512eff69", "test@test.com", true, true, false, null, "", null, "SUPERADMIN", "AQAAAAIAAYagAAAAEJ9IIXyLXjUKaWhWFetMxXKZSRZZF7g+Td9WPM8JPPYKlisg7bEh2RzThCw3hV28UA==", "", null, false, "2298f41b-370f-41de-8da8-f8a128f6641a", "", false, "SuperAdmin" });

            migrationBuilder.InsertData(
                table: "GMSettings",
                columns: new[] { "Id", "BaudRate", "BlackoutPeriod", "CriticalValue", "Duration", "HighValue", "NormalValue", "RepetitionInterval", "SerialPortName" },
                values: new object[] { 1, 57600, 30, 0.80000000000000004, 60, 0.5, 0.0, 2, "COM6" });

            migrationBuilder.InsertData(
                table: "GPSSettings",
                columns: new[] { "Id", "BaudRate", "HeightEnabled", "MaxMoveSpeed", "MinMoveSpeed", "MoveSpeedColor", "MoveSpeedEnabled", "SerialPortName" },
                values: new object[] { 1, 9600, true, 0.0, 0.0, 0, false, "COM5" });

            migrationBuilder.InsertData(
                table: "MeteoStationSettings",
                columns: new[] { "Id", "BaudRate", "HumidityEnabled", "PreassureEnabled", "SerialPortName", "TemperatureEnabled", "WindDirectionEnabled", "WindSpeedEnabled" },
                values: new object[] { 1, 57600, true, true, "COM8", true, true, true });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "0bdaa8f0-aaed-41e7-97b1-d5e7ab5b5531", "9807a71f-c6d8-4d2d-8d06-079ebae45450" },
                    { "19a20dd4-32f2-4029-bc68-8344ee3fecdf", "9807a71f-c6d8-4d2d-8d06-079ebae45450" },
                    { "6d12138c-7929-4dfd-b4e9-a76be12e3ed7", "9807a71f-c6d8-4d2d-8d06-079ebae45450" }
                });

            migrationBuilder.InsertData(
                table: "DetectorSettings",
                columns: new[] { "Id", "Color", "CriticalValue", "EquipmentIdentifier", "GmSettingsId", "HighValue", "IsEnabled", "Name", "NormalValue", "SerialNumber" },
                values: new object[,]
                {
                    { 1, 255, 0.80000000000000004, 1, 1, 0.5, true, "Detector1", 0.0, 3L },
                    { 2, 255, 0.80000000000000004, 3, 1, 0.5, true, "Detector2", 0.0, 3L },
                    { 3, 255, 0.80000000000000004, 255, 1, 0.5, false, "Detector3", 0.0, 3L },
                    { 4, 255, 0.80000000000000004, 255, 1, 0.5, false, "Detector4", 0.0, 3L },
                    { 5, 255, 0.80000000000000004, 255, 1, 0.5, false, "Detector5", 0.0, 3L },
                    { 6, 255, 0.80000000000000004, 255, 1, 0.5, false, "Detector6", 0.0, 3L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DetectorSettings_GmSettingsId",
                table: "DetectorSettings",
                column: "GmSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_MeteoData_Date",
                table: "MeteoData",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_MeteoData_MeteoStreamId",
                table: "MeteoData",
                column: "MeteoStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_MeteoStreams_UserId",
                table: "MeteoStreams",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentity_UserId",
                table: "UserIdentity",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DetectorSettings");

            migrationBuilder.DropTable(
                name: "GPSSettings");

            migrationBuilder.DropTable(
                name: "MeteoData");

            migrationBuilder.DropTable(
                name: "MeteoStationSettings");

            migrationBuilder.DropTable(
                name: "UserIdentity");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GMSettings");

            migrationBuilder.DropTable(
                name: "MeteoStreams");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
