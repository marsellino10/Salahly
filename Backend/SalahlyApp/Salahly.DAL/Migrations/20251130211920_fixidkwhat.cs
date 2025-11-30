using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixidkwhat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Craftsmen",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Craftsmen");
        }
    }
}
