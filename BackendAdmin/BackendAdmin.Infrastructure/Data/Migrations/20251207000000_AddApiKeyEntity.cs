using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TK00001",
                columns: table => new
                {
                    kc1 = table.Column<Guid>(type: "uuid", nullable: false),
                    kc2 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    kc3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kc4 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kc5 = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    kc6 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kc7 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kc8 = table.Column<bool>(type: "boolean", nullable: false),
                    kc9 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    kc10 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kc11 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TK00001", x => x.kc1);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TK00001_kc2",
                table: "TK00001",
                column: "kc2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TK00001_kc3",
                table: "TK00001",
                column: "kc3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TK00001");
        }
    }
}
