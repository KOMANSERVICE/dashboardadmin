using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCashFlowReversalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "fld36",
                table: "TC00001",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "fld37",
                table: "TC00001",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "fld38",
                table: "TC00001",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_fld36",
                table: "TC00001",
                column: "fld36");

            migrationBuilder.AddForeignKey(
                name: "FK_TC00001_TC00001_fld36",
                table: "TC00001",
                column: "fld36",
                principalTable: "TC00001",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TC00001_TC00001_fld36",
                table: "TC00001");

            migrationBuilder.DropIndex(
                name: "IX_TC00001_fld36",
                table: "TC00001");

            migrationBuilder.DropColumn(
                name: "fld36",
                table: "TC00001");

            migrationBuilder.DropColumn(
                name: "fld37",
                table: "TC00001");

            migrationBuilder.DropColumn(
                name: "fld38",
                table: "TC00001");
        }
    }
}
