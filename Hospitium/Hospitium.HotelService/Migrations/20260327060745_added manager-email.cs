using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospitium.HotelService.Migrations
{
    /// <inheritdoc />
    public partial class addedmanageremail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerEmail",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerEmail",
                table: "Hotels");
        }
    }
}
