using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagasinService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.ch1);
                    table.ForeignKey(
                        name: "FK_StockMovements_StockLocations_DestinationLocationId",
                        column: x => x.DestinationLocationId,
                        principalTable: "StockLocations",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_StockLocations_SourceLocationId",
                        column: x => x.SourceLocationId,
                        principalTable: "StockLocations",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockSlips",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BoutiqueId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsInbound = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSlips", x => x.ch1);
                });

            migrationBuilder.CreateTable(
                name: "StockSlipItems",
                columns: table => new
                {
                    ch1 = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StockSlipId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockMovementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSlipItems", x => x.ch1);
                    table.ForeignKey(
                        name: "FK_StockSlipItems_StockMovements_StockMovementId",
                        column: x => x.StockMovementId,
                        principalTable: "StockMovements",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockSlipItems_StockSlips_StockSlipId",
                        column: x => x.StockSlipId,
                        principalTable: "StockSlips",
                        principalColumn: "ch1",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DestinationLocationId",
                table: "StockMovements",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_SourceLocationId",
                table: "StockMovements",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipItems_StockMovementId",
                table: "StockSlipItems",
                column: "StockMovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipItems_StockSlipId",
                table: "StockSlipItems",
                column: "StockSlipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSlipItems");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockSlips");
        }
    }
}
