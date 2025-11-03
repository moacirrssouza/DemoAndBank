using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AndBank.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    position_id = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<string>(type: "text", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => new { x.position_id, x.date });
                });

            migrationBuilder.CreateIndex(
                name: "idx_positions_client",
                table: "positions",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "idx_positions_positionid_date",
                table: "positions",
                columns: new[] { "position_id", "date" });

            migrationBuilder.CreateIndex(
                name: "idx_positions_value",
                table: "positions",
                column: "value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
