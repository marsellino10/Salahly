using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class compositeKeyCraftsmanServiceArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CraftsmanServiceAreas",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_CraftsmanServiceAreas_AreaId",
                table: "CraftsmanServiceAreas");

            migrationBuilder.DropColumn(
                name: "CraftsmanServiceAreaId",
                table: "CraftsmanServiceAreas");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "CraftsmanServiceAreas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CraftsmanServiceAreas",
                table: "CraftsmanServiceAreas",
                columns: new[] { "AreaId", "CraftsmanId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CraftsmanServiceAreas",
                table: "CraftsmanServiceAreas");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "CraftsmanServiceAreas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CraftsmanServiceAreaId",
                table: "CraftsmanServiceAreas",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CraftsmanServiceAreas",
                table: "CraftsmanServiceAreas",
                column: "CraftsmanServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CraftsmanServiceAreas_AreaId",
                table: "CraftsmanServiceAreas",
                column: "AreaId");
        }
    }
}
