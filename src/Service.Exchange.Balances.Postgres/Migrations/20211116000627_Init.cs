using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Exchange.Balances.Postgres.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exchange_balances");

            migrationBuilder.CreateTable(
                name: "balances",
                schema: "exchange_balances",
                columns: table => new
                {
                    WalletIdAssetId = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    WalletId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(20)", precision: 20, nullable: false),
                    ReserveBalance = table.Column<decimal>(type: "numeric(20)", precision: 20, nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balances", x => x.WalletIdAssetId);
                });

            migrationBuilder.CreateTable(
                name: "processed_operations",
                schema: "exchange_balances",
                columns: table => new
                {
                    OperationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    ProcessedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_operations", x => x.OperationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_balances_WalletId",
                schema: "exchange_balances",
                table: "balances",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_balances_WalletId_AssetId",
                schema: "exchange_balances",
                table: "balances",
                columns: new[] { "WalletId", "AssetId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balances",
                schema: "exchange_balances");

            migrationBuilder.DropTable(
                name: "processed_operations",
                schema: "exchange_balances");
        }
    }
}
