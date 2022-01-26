using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Exchange.Balances.Postgres.Migrations
{
    public partial class Addfuturesreservebalances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReserveFuturesOrders",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(40,20)",
                precision: 40,
                scale: 20,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReserveFuturesPositions",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(40,20)",
                precision: 40,
                scale: 20,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReserveFuturesOrders",
                schema: "exchange_balances",
                table: "balances");

            migrationBuilder.DropColumn(
                name: "ReserveFuturesPositions",
                schema: "exchange_balances",
                table: "balances");
        }
    }
}
