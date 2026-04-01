using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospitium.BookingService.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberOfRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Bookings");
        }
    }
}
