using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class changementnomchamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "champ5",
                table: "TM00001",
                newName: "ch5");

            migrationBuilder.RenameColumn(
                name: "champ4",
                table: "TM00001",
                newName: "ch4");

            migrationBuilder.RenameColumn(
                name: "champ3",
                table: "TM00001",
                newName: "ch3");

            migrationBuilder.RenameColumn(
                name: "champ2",
                table: "TM00001",
                newName: "ch2");

            migrationBuilder.RenameColumn(
                name: "champ1",
                table: "TM00001",
                newName: "ch1");

            migrationBuilder.RenameColumn(
                name: "champ5",
                table: "TA00001",
                newName: "ch5");

            migrationBuilder.RenameColumn(
                name: "champ4",
                table: "TA00001",
                newName: "ch4");

            migrationBuilder.RenameColumn(
                name: "champ3",
                table: "TA00001",
                newName: "ch3");

            migrationBuilder.RenameColumn(
                name: "champ2",
                table: "TA00001",
                newName: "ch2");

            migrationBuilder.RenameColumn(
                name: "champ1",
                table: "TA00001",
                newName: "ch1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ch5",
                table: "TM00001",
                newName: "champ5");

            migrationBuilder.RenameColumn(
                name: "ch4",
                table: "TM00001",
                newName: "champ4");

            migrationBuilder.RenameColumn(
                name: "ch3",
                table: "TM00001",
                newName: "champ3");

            migrationBuilder.RenameColumn(
                name: "ch2",
                table: "TM00001",
                newName: "champ2");

            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "TM00001",
                newName: "champ1");

            migrationBuilder.RenameColumn(
                name: "ch5",
                table: "TA00001",
                newName: "champ5");

            migrationBuilder.RenameColumn(
                name: "ch4",
                table: "TA00001",
                newName: "champ4");

            migrationBuilder.RenameColumn(
                name: "ch3",
                table: "TA00001",
                newName: "champ3");

            migrationBuilder.RenameColumn(
                name: "ch2",
                table: "TA00001",
                newName: "champ2");

            migrationBuilder.RenameColumn(
                name: "ch1",
                table: "TA00001",
                newName: "champ1");
        }
    }
}
