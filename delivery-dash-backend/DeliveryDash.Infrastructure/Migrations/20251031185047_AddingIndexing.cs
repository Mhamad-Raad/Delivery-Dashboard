using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_BuildingId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                newName: "IX_Roles_NormalizedName");

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshToken",
                table: "AspNetUsers",
                column: "RefreshToken",
                filter: "\"RefreshToken\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Search_Composite",
                table: "AspNetUsers",
                columns: new[] { "Email", "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuildingId",
                table: "Addresses",
                column: "BuildingId",
                filter: "\"BuildingId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId",
                unique: true,
                filter: "\"UserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId_BuildingId",
                table: "Addresses",
                columns: new[] { "UserId", "BuildingId" },
                filter: "\"UserId\" IS NOT NULL AND \"BuildingId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropIndex(
                name: "IX_Users_RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Users_Search_Composite",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BuildingId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId_BuildingId",
                table: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_NormalizedName",
                table: "AspNetRoles",
                newName: "RoleNameIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BuildingId",
                table: "Addresses",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId",
                unique: true);
        }
    }
}
