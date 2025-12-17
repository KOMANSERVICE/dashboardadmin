using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringCashFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TR00001",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<string>(type: "text", nullable: false),
                    fld3 = table.Column<int>(type: "integer", nullable: false),
                    fld4 = table.Column<string>(type: "text", nullable: false),
                    fld5 = table.Column<string>(type: "text", nullable: false),
                    fld6 = table.Column<string>(type: "text", nullable: true),
                    fld7 = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    fld8 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld9 = table.Column<string>(type: "text", nullable: false),
                    fld10 = table.Column<string>(type: "text", nullable: true),
                    fld11 = table.Column<int>(type: "integer", nullable: false),
                    fld12 = table.Column<int>(type: "integer", nullable: false),
                    fld13 = table.Column<int>(type: "integer", nullable: true),
                    fld14 = table.Column<int>(type: "integer", nullable: true),
                    fld15 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fld16 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fld17 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fld18 = table.Column<bool>(type: "boolean", nullable: false),
                    fld19 = table.Column<bool>(type: "boolean", nullable: false),
                    fld20 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TR00001", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TR00001_TA00001_fld8",
                        column: x => x.fld8,
                        principalTable: "TA00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TR00001_fld8",
                table: "TR00001",
                column: "fld8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TR00001");
        }
    }
}
