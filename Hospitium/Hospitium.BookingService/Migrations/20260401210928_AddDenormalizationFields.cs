using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospitium.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddDenormalizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HotelName",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoomType",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HotelName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Bookings");
        }
    }
}
