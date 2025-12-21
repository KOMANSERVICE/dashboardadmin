using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TA00001",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<string>(type: "text", nullable: false),
                    fld3 = table.Column<string>(type: "text", nullable: false),
                    fld14 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fld4 = table.Column<int>(type: "integer", nullable: false),
                    fld5 = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fld6 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fld7 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld8 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld9 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fld10 = table.Column<bool>(type: "boolean", nullable: false),
                    fld11 = table.Column<bool>(type: "boolean", nullable: false),
                    fld12 = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    fld13 = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TA00001", x => x.ch1);
                });

            migrationBuilder.CreateTable(
                name: "TC00003",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<string>(type: "text", nullable: false),
                    fld3 = table.Column<int>(type: "integer", nullable: false),
                    fld4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fld5 = table.Column<bool>(type: "boolean", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TC00003", x => x.ch1);
                });

            migrationBuilder.CreateTable(
                name: "TC00001",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<string>(type: "text", nullable: false),
                    fld3 = table.Column<string>(type: "text", nullable: true),
                    fld4 = table.Column<int>(type: "integer", nullable: false),
                    fld5 = table.Column<string>(type: "text", nullable: false),
                    fld6 = table.Column<string>(type: "text", nullable: false),
                    fld7 = table.Column<string>(type: "text", nullable: true),
                    fld8 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld9 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld10 = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    fld11 = table.Column<string>(type: "text", nullable: false),
                    fld12 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld13 = table.Column<Guid>(type: "uuid", nullable: true),
                    fld14 = table.Column<string>(type: "text", nullable: false),
                    fld15 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fld16 = table.Column<int>(type: "integer", nullable: true),
                    fld17 = table.Column<string>(type: "text", nullable: true),
                    fld18 = table.Column<string>(type: "text", nullable: true),
                    fld19 = table.Column<int>(type: "integer", nullable: false),
                    fld20 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fld21 = table.Column<string>(type: "text", nullable: true),
                    fld22 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fld23 = table.Column<string>(type: "text", nullable: true),
                    fld24 = table.Column<string>(type: "text", nullable: true),
                    fld25 = table.Column<bool>(type: "boolean", nullable: false),
                    fld26 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fld27 = table.Column<string>(type: "text", nullable: true),
                    fld28 = table.Column<string>(type: "text", nullable: true),
                    fld29 = table.Column<bool>(type: "boolean", nullable: false),
                    fld30 = table.Column<string>(type: "text", nullable: true),
                    fld31 = table.Column<string>(type: "text", nullable: true),
                    fld32 = table.Column<string>(type: "text", nullable: true),
                    fld33 = table.Column<string>(type: "text", nullable: true),
                    fld34 = table.Column<bool>(type: "boolean", nullable: false),
                    fld35 = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TC00001", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TC00001_TA00001_fld12",
                        column: x => x.fld12,
                        principalTable: "TA00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TC00001_TA00001_fld13",
                        column: x => x.fld13,
                        principalTable: "TA00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TC00001_TC00003_CategoryId1",
                        column: x => x.CategoryId1,
                        principalTable: "TC00003",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TC00002",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<int>(type: "integer", nullable: false),
                    fld3 = table.Column<string>(type: "text", nullable: true),
                    fld4 = table.Column<string>(type: "text", nullable: true),
                    fld25 = table.Column<string>(type: "text", nullable: true),
                    CashFlowId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TC00002", x => x.ch1);
                    table.ForeignKey(
                        name: "FK_TC00002_TC00001_CashFlowId1",
                        column: x => x.CashFlowId1,
                        principalTable: "TC00001",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_CategoryId1",
                table: "TC00001",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_fld12",
                table: "TC00001",
                column: "fld12");

            migrationBuilder.CreateIndex(
                name: "IX_TC00001_fld13",
                table: "TC00001",
                column: "fld13");

            migrationBuilder.CreateIndex(
                name: "IX_TC00002_CashFlowId1",
                table: "TC00002",
                column: "CashFlowId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TC00002");

            migrationBuilder.DropTable(
                name: "TC00001");

            migrationBuilder.DropTable(
                name: "TA00001");

            migrationBuilder.DropTable(
                name: "TC00003");
        }
    }
}
