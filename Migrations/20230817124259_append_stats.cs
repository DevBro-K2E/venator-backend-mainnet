using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class append_stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoundElapsedSecs",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AliveSecs",
                table: "GameLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundElapsedSecs",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AliveSecs",
                table: "GameLogs");
        }
    }
}
