using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAdmin.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppAdminAnonimeNomChamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TA00001",
                newName: "Champ5");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "TA00001",
                newName: "Champ3");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "TA00001",
                newName: "Champ7");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TA00001",
                newName: "Champ6");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "TA00001",
                newName: "Champ9");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TA00001",
                newName: "Champ8");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TA00001",
                newName: "Champ4");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TA00001",
                newName: "Champ2");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TA00001",
                newName: "Champ1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Champ9",
                table: "TA00001",
                newName: "Link");

            migrationBuilder.RenameColumn(
                name: "Champ8",
                table: "TA00001",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Champ7",
                table: "TA00001",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "Champ6",
                table: "TA00001",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Champ5",
                table: "TA00001",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "Champ4",
                table: "TA00001",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "Champ3",
                table: "TA00001",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Champ2",
                table: "TA00001",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Champ1",
                table: "TA00001",
                newName: "Id");
        }
    }
}
