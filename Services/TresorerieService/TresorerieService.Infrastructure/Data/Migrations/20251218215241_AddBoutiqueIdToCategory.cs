using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresorerieService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBoutiqueIdToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fld6",
                table: "TC00003",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fld6",
                table: "TC00003");
        }
    }
}
