using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Menu.Grpc.Migrations
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
                    ch1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    cf1 = table.Column<string>(type: "TEXT", nullable: false),
                    cf2 = table.Column<string>(type: "TEXT", nullable: false),
                    cf3 = table.Column<string>(type: "TEXT", nullable: false),
                    cf4 = table.Column<string>(type: "TEXT", nullable: false),
                    cf6 = table.Column<bool>(type: "INTEGER", nullable: false),
                    cf5 = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM00001", x => x.ch1);
                });

            migrationBuilder.InsertData(
                table: "TM00001",
                columns: new[] { "ch1", "cf2", "cf5", "cf4", "cf6", "cf1", "cf3" },
                values: new object[,]
                {
                    { new Guid("1cb25cbe-1f2f-4fc8-bf53-25255dbd9b68"), "/product", new Guid("b9bd41f4-cb78-4d71-b2a2-08d385944f15"), "fa-solid fa-bag-shopping", true, "Produit", "/produit" },
                    { new Guid("5d5b22a1-dc69-4b66-811a-9b4598b71f00"), "/sale", new Guid("2c252630-2e4f-4d09-b2d6-057e4f04eff1"), "fa-solid fa-chart-pie", true, "Tableau de bord", "/dashboard" }
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
