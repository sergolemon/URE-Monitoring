using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URE.Core.Migrations
{
    /// <inheritdoc />
    public partial class addittionalMeteoStationConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipmentIdentifier",
                table: "MeteoStationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SerialNumber",
                table: "MeteoStationSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7b5e6ab8-8001-423c-aac2-17b953652cb8", "AQAAAAIAAYagAAAAEFCJQwPhZXV7oPwYM1ef/7YqdVChO0t5Wv9y3YZWISweUuASUjV8sqbu+u/W0w1i/g==", "9fcbefd8-f1ca-4d89-b64d-afe916390daa" });

            migrationBuilder.UpdateData(
                table: "MeteoStationSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EquipmentIdentifier", "SerialNumber" },
                values: new object[] { 0, 0L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentIdentifier",
                table: "MeteoStationSettings");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "MeteoStationSettings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "40ed038c-0319-489e-8f33-ba40512eff69", "AQAAAAIAAYagAAAAEJ9IIXyLXjUKaWhWFetMxXKZSRZZF7g+Td9WPM8JPPYKlisg7bEh2RzThCw3hV28UA==", "2298f41b-370f-41de-8da8-f8a128f6641a" });
        }
    }
}
