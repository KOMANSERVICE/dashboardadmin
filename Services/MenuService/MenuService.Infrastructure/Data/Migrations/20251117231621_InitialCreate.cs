using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TM00001",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf1 = table.Column<string>(type: "text", nullable: false),
                    cf2 = table.Column<string>(type: "text", nullable: false),
                    cf3 = table.Column<string>(type: "text", nullable: false),
                    cf4 = table.Column<string>(type: "text", nullable: false),
                    cf5 = table.Column<bool>(type: "boolean", nullable: false),
                    cf6 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM00001", x => x.ch1);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM00001");
        }
    }
}
