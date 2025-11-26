using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class adminVerify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Craftsmen");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Craftsmen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Craftsmen",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Craftsmen");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Craftsmen");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "Craftsmen",
                type: "datetime2",
                nullable: true);
        }
    }
}
