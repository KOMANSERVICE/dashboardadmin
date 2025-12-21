using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceResourceConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TK00002",
                columns: table => new
                {
                    rc1 = table.Column<Guid>(type: "uuid", nullable: false),
                    rc2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rc3 = table.Column<long>(type: "bigint", nullable: true),
                    rc4 = table.Column<long>(type: "bigint", nullable: true),
                    rc5 = table.Column<long>(type: "bigint", nullable: true),
                    rc6 = table.Column<long>(type: "bigint", nullable: true),
                    rc7 = table.Column<long>(type: "bigint", nullable: true),
                    rc8 = table.Column<int>(type: "integer", nullable: true),
                    rc9 = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    rc10 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rc11 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TK00002", x => x.rc1);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TK00002_rc2",
                table: "TK00002",
                column: "rc2",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TK00002");
        }
    }
}
