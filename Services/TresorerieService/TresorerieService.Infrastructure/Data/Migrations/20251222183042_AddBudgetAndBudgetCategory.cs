using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAndBudgetCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB00001",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    application_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    boutique_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    allocated_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    alert_threshold = table.Column<int>(type: "integer", nullable: false),
                    is_exceeded = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB00001", x => x.ch1);
                });

            migrationBuilder.CreateTable(
                name: "TB00002",
                columns: table => new
                {
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB00002", x => new { x.budget_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_TB00002_TB00001_budget_id",
                        column: x => x.budget_id,
                        principalTable: "TB00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TB00002_TC00003_category_id",
                        column: x => x.category_id,
                        principalTable: "TC00003",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB00002_category_id",
                table: "TB00002",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB00002");

            migrationBuilder.DropTable(
                name: "TB00001");
        }
    }
}
