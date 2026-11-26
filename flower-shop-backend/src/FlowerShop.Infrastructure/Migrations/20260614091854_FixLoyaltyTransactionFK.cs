using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowerShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLoyaltyTransactionFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_loyalty_accounts_UserId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_orders_OrderId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_users_UserId1",
                table: "LoyaltyTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyTransactions_UserId1",
                table: "LoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "LoyaltyTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "LoyaltyTransactions",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "LoyaltyAccountId",
                table: "LoyaltyTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_LoyaltyAccountId",
                table: "LoyaltyTransactions",
                column: "LoyaltyAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_loyalty_accounts_LoyaltyAccountId",
                table: "LoyaltyTransactions",
                column: "LoyaltyAccountId",
                principalTable: "loyalty_accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_orders_OrderId",
                table: "LoyaltyTransactions",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_users_UserId",
                table: "LoyaltyTransactions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_loyalty_accounts_LoyaltyAccountId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_orders_OrderId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyTransactions_users_UserId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyTransactions_LoyaltyAccountId",
                table: "LoyaltyTransactions");

            migrationBuilder.DropColumn(
                name: "LoyaltyAccountId",
                table: "LoyaltyTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "LoyaltyTransactions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "LoyaltyTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_UserId1",
                table: "LoyaltyTransactions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_loyalty_accounts_UserId",
                table: "LoyaltyTransactions",
                column: "UserId",
                principalTable: "loyalty_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_orders_OrderId",
                table: "LoyaltyTransactions",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyTransactions_users_UserId1",
                table: "LoyaltyTransactions",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
