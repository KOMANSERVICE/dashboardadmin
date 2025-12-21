using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryId1ShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TC00001_TC00003_CategoryId1",
                table: "TC00001");

            migrationBuilder.DropIndex(
                name: "IX_TC00001_CategoryId1",
                table: "TC00001");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "TC00001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "TC00001",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_CategoryId1",
                table: "TC00001",
                column: "CategoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TC00001_TC00003_CategoryId1",
                table: "TC00001",
                column: "CategoryId1",
                principalTable: "TC00003",
                principalColumn: "ch1",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
