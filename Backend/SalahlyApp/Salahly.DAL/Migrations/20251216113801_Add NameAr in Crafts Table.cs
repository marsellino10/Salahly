using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNameArinCraftsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Crafts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Crafts");
        }
    }
}
