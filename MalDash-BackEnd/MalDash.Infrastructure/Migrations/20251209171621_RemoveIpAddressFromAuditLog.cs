using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIpAddressFromAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);
        }
    }
}
