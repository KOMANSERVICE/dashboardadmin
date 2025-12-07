using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TM00002",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf1 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    cf2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cf3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cf4 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    cf5 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cf6 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cf7 = table.Column<bool>(type: "boolean", nullable: false),
                    cf8 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cf9 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cf10 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cf11 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cf12 = table.Column<string>(type: "text", nullable: false),
                    cf13 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM00002", x => x.ch1);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM00002_cf1",
                table: "TM00002",
                column: "cf1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TM00002_cf2",
                table: "TM00002",
                column: "cf2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM00002");
        }
    }
}
