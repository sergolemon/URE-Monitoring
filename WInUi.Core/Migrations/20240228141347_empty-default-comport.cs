using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URE.Core.Migrations
{
    /// <inheritdoc />
    public partial class emptydefaultcomport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "51534696-c16f-445c-8e82-2a0e183eb4ad", "AQAAAAIAAYagAAAAEP5EMt/b0t37iptKpbOr+hHCPyxppmPT/G55ThnJWARJostonbW/CqwWvr71Tqr6NA==", "4c99fc3f-dacb-49da-8384-40142faec96c" });

            migrationBuilder.UpdateData(
                table: "GMSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "");

            migrationBuilder.UpdateData(
                table: "GPSSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "");

            migrationBuilder.UpdateData(
                table: "MeteoStationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "9807a71f-c6d8-4d2d-8d06-079ebae45450",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "40ed038c-0319-489e-8f33-ba40512eff69", "AQAAAAIAAYagAAAAEJ9IIXyLXjUKaWhWFetMxXKZSRZZF7g+Td9WPM8JPPYKlisg7bEh2RzThCw3hV28UA==", "2298f41b-370f-41de-8da8-f8a128f6641a" });

            migrationBuilder.UpdateData(
                table: "GMSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "COM6");

            migrationBuilder.UpdateData(
                table: "GPSSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "COM5");

            migrationBuilder.UpdateData(
                table: "MeteoStationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "SerialPortName",
                value: "COM8");
        }
    }
}
