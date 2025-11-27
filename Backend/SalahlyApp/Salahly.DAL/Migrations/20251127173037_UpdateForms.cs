using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredTimeSlot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AvailableFromDate",
                table: "CraftsmanOffers");

            migrationBuilder.RenameColumn(
                name: "PreferredDate",
                table: "ServiceRequests",
                newName: "AvailableToDate");

            migrationBuilder.RenameColumn(
                name: "AvailableToDate",
                table: "CraftsmanOffers",
                newName: "PreferredDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFromDate",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeSlot",
                table: "CraftsmanOffers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFromDate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PreferredTimeSlot",
                table: "CraftsmanOffers");

            migrationBuilder.RenameColumn(
                name: "AvailableToDate",
                table: "ServiceRequests",
                newName: "PreferredDate");

            migrationBuilder.RenameColumn(
                name: "PreferredDate",
                table: "CraftsmanOffers",
                newName: "AvailableToDate");

            migrationBuilder.AddColumn<string>(
                name: "PreferredTimeSlot",
                table: "ServiceRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFromDate",
                table: "CraftsmanOffers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
