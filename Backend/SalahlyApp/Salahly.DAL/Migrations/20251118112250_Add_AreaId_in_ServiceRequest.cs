using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salahly.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_AreaId_in_ServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_Location_Status",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ServiceRequests");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Location_Status",
                table: "ServiceRequests",
                columns: new[] { "AreaId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Areas_AreaId",
                table: "ServiceRequests",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Areas_AreaId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_Location_Status",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "ServiceRequests");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_Location_Status",
                table: "ServiceRequests",
                columns: new[] { "City", "Area", "Status" });
        }
    }
}
