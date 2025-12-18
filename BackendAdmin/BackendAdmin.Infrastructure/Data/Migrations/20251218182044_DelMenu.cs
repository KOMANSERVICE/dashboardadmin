using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DelMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM00001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TM00001",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf5 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf2 = table.Column<string>(type: "text", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    cf4 = table.Column<string>(type: "text", nullable: false),
                    cf6 = table.Column<bool>(type: "boolean", nullable: false),
                    cf1 = table.Column<string>(type: "text", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false),
                    cf3 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM00001", x => x.ch1);
                    table.ForeignKey(
                        name: "FK_TM00001_TA00001_cf5",
                        column: x => x.cf5,
                        principalTable: "TA00001",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM00001_cf5",
                table: "TM00001",
                column: "cf5");
        }
    }
}
