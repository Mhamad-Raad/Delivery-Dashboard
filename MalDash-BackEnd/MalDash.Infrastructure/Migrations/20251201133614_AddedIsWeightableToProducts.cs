using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsWeightableToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWeightable",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWeightable",
                table: "Products");
        }
    }
}
