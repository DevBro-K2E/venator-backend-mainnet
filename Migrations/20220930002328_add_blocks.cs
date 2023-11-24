using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class add_blocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "WalletTransactions");

            migrationBuilder.CreateTable(
                name: "WalletBlocks",
                columns: table => new
                {
                    BlockId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBlocks", x => x.BlockId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletBlocks");

            migrationBuilder.AddColumn<long>(
                name: "BlockId",
                table: "WalletTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
