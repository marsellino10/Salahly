using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class newModelAreaAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CraftsmanServiceAreas_Location_Active",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_CraftsmanServiceAreas_Unique",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CraftsmanServiceAreas");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "CraftsmanServiceAreas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CraftsmanServiceAreas_AreaId",
                table: "CraftsmanServiceAreas",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Region_City_Unique",
                table: "Areas",
                columns: new[] { "Region", "City" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CraftsmanServiceAreas_Areas_AreaId",
                table: "CraftsmanServiceAreas",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CraftsmanServiceAreas_Areas_AreaId",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_CraftsmanServiceAreas_AreaId",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "CraftsmanServiceAreas");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "CraftsmanServiceAreas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CraftsmanServiceAreas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CraftsmanServiceAreas_Location_Active",
                table: "CraftsmanServiceAreas",
                columns: new[] { "City", "Area", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CraftsmanServiceAreas_Unique",
                table: "CraftsmanServiceAreas",
                columns: new[] { "CraftsmanId", "City", "Area" },
                unique: true);
        }
    }
}
