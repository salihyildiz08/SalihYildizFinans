using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalihYildizFinans.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserIdToBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Banks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_AppUserId",
                table: "Banks",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banks_AspNetUsers_AppUserId",
                table: "Banks",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banks_AspNetUsers_AppUserId",
                table: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_Banks_AppUserId",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Banks");
        }
    }
}
