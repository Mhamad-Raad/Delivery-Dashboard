using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryDash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusTrackingAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderAssignments_OrderId",
                table: "OrderAssignments");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutForDeliveryAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparingAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            // xmin is a Postgres system column present on every table.
            // Do NOT AddColumn it — EF's shadow property just references the existing system column for concurrency.

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveredAt",
                table: "Orders",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "UX_OrderAssignments_OrderId_Accepted",
                table: "OrderAssignments",
                column: "OrderId",
                unique: true,
                filter: "\"Status\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveredAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "UX_OrderAssignments_OrderId_Accepted",
                table: "OrderAssignments");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OutForDeliveryAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreparingAt",
                table: "Orders");

            // xmin is a Postgres system column — nothing to drop.

            migrationBuilder.CreateIndex(
                name: "IX_OrderAssignments_OrderId",
                table: "OrderAssignments",
                column: "OrderId");
        }
    }
}
