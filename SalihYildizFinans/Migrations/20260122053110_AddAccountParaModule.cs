using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalihYildizFinans.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountParaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountParas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountParas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountParas_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountParaDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountParaId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountParaDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountParaDetails_AccountParas_AccountParaId",
                        column: x => x.AccountParaId,
                        principalTable: "AccountParas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountParaDetails_AccountParaId",
                table: "AccountParaDetails",
                column: "AccountParaId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountParas_AppUserId",
                table: "AccountParas",
                column: "AppUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountParaDetails");

            migrationBuilder.DropTable(
                name: "AccountParas");
        }
    }
}
