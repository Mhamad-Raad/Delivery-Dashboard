using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToUserAndVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vendors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_CreatedAt",
                table: "Vendors",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendors_CreatedAt",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");
        }
    }
}
