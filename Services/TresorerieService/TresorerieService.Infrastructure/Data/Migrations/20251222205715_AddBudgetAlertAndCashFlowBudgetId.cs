using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAlertAndCashFlowBudgetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "budget_id",
                table: "TC00001",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TB00003",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld2 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fld4 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld5 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld6 = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    fld7 = table.Column<int>(type: "integer", nullable: false),
                    fld8 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fld9 = table.Column<bool>(type: "boolean", nullable: false),
                    fld10 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fld11 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB00003", x => x.ch1);
                    table.ForeignKey(
                        name: "FK_TB00003_TB00001_fld1",
                        column: x => x.fld1,
                        principalTable: "TB00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB00003_TC00001_fld2",
                        column: x => x.fld2,
                        principalTable: "TC00001",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_budget_id",
                table: "TC00001",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "IX_TB00003_fld1",
                table: "TB00003",
                column: "fld1");

            migrationBuilder.CreateIndex(
                name: "IX_TB00003_fld2",
                table: "TB00003",
                column: "fld2");

            migrationBuilder.AddForeignKey(
                name: "FK_TC00001_TB00001_budget_id",
                table: "TC00001",
                column: "budget_id",
                principalTable: "TB00001",
                principalColumn: "ch1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TC00001_TB00001_budget_id",
                table: "TC00001");

            migrationBuilder.DropTable(
                name: "TB00003");

            migrationBuilder.DropIndex(
                name: "IX_TC00001_budget_id",
                table: "TC00001");

            migrationBuilder.DropColumn(
                name: "budget_id",
                table: "TC00001");
        }
    }
}
