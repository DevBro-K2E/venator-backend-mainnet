using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class update_transactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BlockId",
                table: "WalletTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WalletTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_UserId",
                table: "WalletTransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_AspNetUsers_UserId",
                table: "WalletTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_AspNetUsers_UserId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_UserId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WalletTransactions");
        }
    }
}
