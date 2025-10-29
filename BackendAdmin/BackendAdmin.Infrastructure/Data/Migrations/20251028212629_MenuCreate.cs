using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MenuCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Champ5",
                table: "TA00001",
                newName: "champ5");

            migrationBuilder.RenameColumn(
                name: "Champ4",
                table: "TA00001",
                newName: "champ4");

            migrationBuilder.RenameColumn(
                name: "Champ3",
                table: "TA00001",
                newName: "champ3");

            migrationBuilder.RenameColumn(
                name: "Champ2",
                table: "TA00001",
                newName: "champ2");

            migrationBuilder.RenameColumn(
                name: "Champ1",
                table: "TA00001",
                newName: "champ1");

            migrationBuilder.RenameColumn(
                name: "Champ9",
                table: "TA00001",
                newName: "cf4");

            migrationBuilder.RenameColumn(
                name: "Champ8",
                table: "TA00001",
                newName: "cf3");

            migrationBuilder.RenameColumn(
                name: "Champ7",
                table: "TA00001",
                newName: "cf2");

            migrationBuilder.RenameColumn(
                name: "Champ6",
                table: "TA00001",
                newName: "cf1");

            migrationBuilder.CreateTable(
                name: "TM00001",
                columns: table => new
                {
                    champ1 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf1 = table.Column<Guid>(type: "uuid", nullable: false),
                    cf2 = table.Column<string>(type: "text", nullable: false),
                    cf3 = table.Column<string>(type: "text", nullable: false),
                    cf4 = table.Column<string>(type: "text", nullable: false),
                    cf5 = table.Column<string>(type: "text", nullable: false),
                    cf6 = table.Column<Guid>(type: "uuid", nullable: false),
                    champ2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    champ3 = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    champ4 = table.Column<string>(type: "text", nullable: false),
                    champ5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM00001", x => x.champ1);
                    table.ForeignKey(
                        name: "FK_TM00001_TA00001_cf6",
                        column: x => x.cf6,
                        principalTable: "TA00001",
                        principalColumn: "champ1",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM00001_cf6",
                table: "TM00001",
                column: "cf6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM00001");

            migrationBuilder.RenameColumn(
                name: "champ5",
                table: "TA00001",
                newName: "Champ5");

            migrationBuilder.RenameColumn(
                name: "champ4",
                table: "TA00001",
                newName: "Champ4");

            migrationBuilder.RenameColumn(
                name: "champ3",
                table: "TA00001",
                newName: "Champ3");

            migrationBuilder.RenameColumn(
                name: "champ2",
                table: "TA00001",
                newName: "Champ2");

            migrationBuilder.RenameColumn(
                name: "champ1",
                table: "TA00001",
                newName: "Champ1");

            migrationBuilder.RenameColumn(
                name: "cf4",
                table: "TA00001",
                newName: "Champ9");

            migrationBuilder.RenameColumn(
                name: "cf3",
                table: "TA00001",
                newName: "Champ8");

            migrationBuilder.RenameColumn(
                name: "cf2",
                table: "TA00001",
                newName: "Champ7");

            migrationBuilder.RenameColumn(
                name: "cf1",
                table: "TA00001",
                newName: "Champ6");
        }
    }
}
