using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedAptNumberToAptName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartments_FloorId_ApartmentNumber",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "ApartmentNumber",
                table: "Apartments");

            migrationBuilder.AddColumn<string>(
                name: "ApartmentName",
                table: "Apartments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_FloorId_ApartmentName",
                table: "Apartments",
                columns: new[] { "FloorId", "ApartmentName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartments_FloorId_ApartmentName",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "ApartmentName",
                table: "Apartments");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentNumber",
                table: "Apartments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_FloorId_ApartmentNumber",
                table: "Apartments",
                columns: new[] { "FloorId", "ApartmentNumber" },
                unique: true);
        }
    }
}
