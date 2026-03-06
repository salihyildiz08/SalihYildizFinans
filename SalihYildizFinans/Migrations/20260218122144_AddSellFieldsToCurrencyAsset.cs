using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalihYildizFinans.Migrations
{
    /// <inheritdoc />
    public partial class AddSellFieldsToCurrencyAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direction",
                table: "CurrencyAssets");

            migrationBuilder.AddColumn<DateTime>(
                name: "SellDate",
                table: "CurrencyAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SellQuantity",
                table: "CurrencyAssets",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellTotalPrice",
                table: "CurrencyAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellUnitPrice",
                table: "CurrencyAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellDate",
                table: "CurrencyAssets");

            migrationBuilder.DropColumn(
                name: "SellQuantity",
                table: "CurrencyAssets");

            migrationBuilder.DropColumn(
                name: "SellTotalPrice",
                table: "CurrencyAssets");

            migrationBuilder.DropColumn(
                name: "SellUnitPrice",
                table: "CurrencyAssets");

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "CurrencyAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
