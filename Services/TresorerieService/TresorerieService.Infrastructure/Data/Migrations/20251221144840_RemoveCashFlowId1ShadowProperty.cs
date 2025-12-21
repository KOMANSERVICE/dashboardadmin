using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCashFlowId1ShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TC00002_TC00001_CashFlowId1",
                table: "TC00002");

            migrationBuilder.DropIndex(
                name: "IX_TC00002_CashFlowId1",
                table: "TC00002");

            migrationBuilder.DropColumn(
                name: "CashFlowId1",
                table: "TC00002");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CashFlowId1",
                table: "TC00002",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TC00002_CashFlowId1",
                table: "TC00002",
                column: "CashFlowId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TC00002_TC00001_CashFlowId1",
                table: "TC00002",
                column: "CashFlowId1",
                principalTable: "TC00001",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
