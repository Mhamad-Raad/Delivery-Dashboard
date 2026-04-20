using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCategorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name_Trgm",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Vendors",
                newName: "VendorCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Vendors_Type",
                table: "Vendors",
                newName: "IX_Vendors_VendorCategoryId");

            migrationBuilder.RenameColumn(
                name: "ParentCategoryId",
                table: "Categories",
                newName: "SortOrder");

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VendorCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Trgm",
                table: "Categories",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_VendorId",
                table: "Categories",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_VendorId_Name",
                table: "Categories",
                columns: new[] { "VendorId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorCategories_IsActive",
                table: "VendorCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_VendorCategories_Name_Trgm",
                table: "VendorCategories",
                column: "Name",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Vendors_VendorId",
                table: "Categories",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_VendorCategories_VendorCategoryId",
                table: "Vendors",
                column: "VendorCategoryId",
                principalTable: "VendorCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Vendors_VendorId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_VendorCategories_VendorCategoryId",
                table: "Vendors");

            migrationBuilder.DropTable(
                name: "VendorCategories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name_Trgm",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_VendorId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_VendorId_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "VendorCategoryId",
                table: "Vendors",
                newName: "Type");

            migrationBuilder.RenameIndex(
                name: "IX_Vendors_VendorCategoryId",
                table: "Vendors",
                newName: "IX_Vendors_Type");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "Categories",
                newName: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name_Trgm",
                table: "Categories",
                column: "Name",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
