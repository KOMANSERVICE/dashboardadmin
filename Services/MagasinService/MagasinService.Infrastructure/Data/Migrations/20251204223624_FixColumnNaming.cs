using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagasinService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "StockSlips",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "StockSlipItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "StockMovements",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "StockLocations",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockSlips",
                newName: "ch1");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockSlipItems",
                newName: "ch1");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockMovements",
                newName: "ch1");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StockLocations",
                newName: "ch1");
        }
    }
}
