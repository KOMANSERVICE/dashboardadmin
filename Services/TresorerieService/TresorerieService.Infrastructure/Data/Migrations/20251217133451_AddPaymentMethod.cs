using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TP00001",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fld1 = table.Column<string>(type: "text", nullable: false),
                    fld2 = table.Column<string>(type: "text", nullable: false),
                    fld3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fld4 = table.Column<int>(type: "integer", nullable: false),
                    fld5 = table.Column<bool>(type: "boolean", nullable: false),
                    fld6 = table.Column<bool>(type: "boolean", nullable: false),
                    ch2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ch4 = table.Column<string>(type: "text", nullable: false),
                    ch5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TP00001", x => x.ch1);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TP00001");
        }
    }
}
