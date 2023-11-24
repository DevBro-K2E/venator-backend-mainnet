using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class metrics_ipaddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserSessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserSessions");
        }
    }
}
