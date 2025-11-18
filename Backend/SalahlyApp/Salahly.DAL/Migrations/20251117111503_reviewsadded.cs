using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class reviewsadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Craftsmen_CraftsmanId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_Craftsman_Rating_Date",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_IsVerified_CreatedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Craftsmen_Craft_Available_Rating",
                table: "Craftsmen");

            migrationBuilder.DropIndex(
                name: "IX_Craftsmen_RatingAverage_IsAvailable",
                table: "Craftsmen");

            migrationBuilder.DropColumn(
                name: "CraftsmanResponse",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ResponsedAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "Craftsmen");

            migrationBuilder.AddColumn<double>(
                name: "RatingAverage",
                table: "Users",
                type: "float(3)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 5.0);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Reviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CraftsmanId",
                table: "Reviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewerUserId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetUserId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CraftsmanId",
                table: "Reviews",
                column: "CraftsmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerUserId",
                table: "Reviews",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TargetUserId",
                table: "Reviews",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Craftsmen_CraftsmanId",
                table: "Reviews",
                column: "CraftsmanId",
                principalTable: "Craftsmen",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_ReviewerUserId",
                table: "Reviews",
                column: "ReviewerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_TargetUserId",
                table: "Reviews",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Craftsmen_CraftsmanId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_ReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_TargetUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CraftsmanId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_TargetUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CraftsmanId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "CraftsmanResponse",
                table: "Reviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponsedAt",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RatingAverage",
                table: "Craftsmen",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Craftsman_Rating_Date",
                table: "Reviews",
                columns: new[] { "CraftsmanId", "Rating", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsVerified_CreatedAt",
                table: "Reviews",
                columns: new[] { "IsVerified", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Craftsmen_Craft_Available_Rating",
                table: "Craftsmen",
                columns: new[] { "CraftId", "IsAvailable", "RatingAverage" });

            migrationBuilder.CreateIndex(
                name: "IX_Craftsmen_RatingAverage_IsAvailable",
                table: "Craftsmen",
                columns: new[] { "RatingAverage", "IsAvailable" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Craftsmen_CraftsmanId",
                table: "Reviews",
                column: "CraftsmanId",
                principalTable: "Craftsmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
