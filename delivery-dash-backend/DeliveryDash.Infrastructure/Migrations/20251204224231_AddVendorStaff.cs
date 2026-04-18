using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VendorStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorStaff_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorStaff_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorStaff_IsActive",
                table: "VendorStaff",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_VendorStaff_UserId",
                table: "VendorStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorStaff_UserId_VendorId",
                table: "VendorStaff",
                columns: new[] { "UserId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorStaff_VendorId",
                table: "VendorStaff",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorStaff");
        }
    }
}
