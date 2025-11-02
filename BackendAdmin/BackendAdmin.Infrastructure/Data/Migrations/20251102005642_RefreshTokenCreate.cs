using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TR00001",
                columns: table => new
                {
                    fc1 = table.Column<Guid>(type: "uuid", nullable: false),
                    fc2 = table.Column<string>(type: "text", nullable: false),
                    fc3 = table.Column<string>(type: "text", nullable: false),
                    fc4 = table.Column<string>(type: "text", nullable: false),
                    fc5 = table.Column<string>(type: "text", nullable: false),
                    fc6 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fc7 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fc8 = table.Column<bool>(type: "boolean", nullable: false),
                    fc9 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TR00001", x => x.fc1);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TR00001");
        }
    }
}
