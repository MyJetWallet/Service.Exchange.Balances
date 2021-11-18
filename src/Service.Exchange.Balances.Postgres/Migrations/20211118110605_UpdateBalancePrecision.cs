using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Exchange.Balances.Postgres.Migrations
{
    public partial class UpdateBalancePrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ReserveBalance",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(40,20)",
                precision: 40,
                scale: 20,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldPrecision: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(40,20)",
                precision: 40,
                scale: 20,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldPrecision: 20);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ReserveBalance",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(20,0)",
                precision: 20,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(40,20)",
                oldPrecision: 40,
                oldScale: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                schema: "exchange_balances",
                table: "balances",
                type: "numeric(20,0)",
                precision: 20,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(40,20)",
                oldPrecision: 40,
                oldScale: 20);
        }
    }
}
