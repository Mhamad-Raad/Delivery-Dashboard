using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GINTrigramIndexForBuildingName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the existing unique B-tree index for uniqueness constraint
            // Just add the GIN trigram index for search performance
            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Name_Trgm",
                table: "Buildings",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_Name_Trgm",
                table: "Buildings");
        }
    }
}