using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Enable_PgTrgm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== ENABLE pg_trgm EXTENSION FIRST =====
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Trgm",
                table: "AspNetUsers",
                column: "Email")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirstName_Trgm",
                table: "AspNetUsers",
                column: "FirstName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastName_Trgm",
                table: "AspNetUsers",
                column: "LastName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber_Trgm",
                table: "AspNetUsers",
                column: "PhoneNumber",
                filter: "\"PhoneNumber\" IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_Trgm",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_FirstName_Trgm",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastName_Trgm",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber_Trgm",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirstName",
                table: "AspNetUsers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastName",
                table: "AspNetUsers",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "AspNetUsers",
                column: "PhoneNumber",
                filter: "\"PhoneNumber\" IS NOT NULL");
        }
    }
}
