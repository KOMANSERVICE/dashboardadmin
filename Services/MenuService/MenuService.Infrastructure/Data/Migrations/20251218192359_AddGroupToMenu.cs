using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cf7",
                table: "TM00001",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cf7",
                table: "TM00001");
        }
    }
}
