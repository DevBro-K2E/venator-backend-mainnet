using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class fix_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLogs_AspNetUsers_UserModelId",
                table: "GameLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_GameLogs_Games_GameModelId",
                table: "GameLogs");

            migrationBuilder.DropIndex(
                name: "IX_GameLogs_GameModelId",
                table: "GameLogs");

            migrationBuilder.DropIndex(
                name: "IX_GameLogs_UserModelId",
                table: "GameLogs");

            migrationBuilder.DropColumn(
                name: "GameModelId",
                table: "GameLogs");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "GameLogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameModelId",
                table: "GameLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserModelId",
                table: "GameLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_GameModelId",
                table: "GameLogs",
                column: "GameModelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_UserModelId",
                table: "GameLogs",
                column: "UserModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLogs_AspNetUsers_UserModelId",
                table: "GameLogs",
                column: "UserModelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLogs_Games_GameModelId",
                table: "GameLogs",
                column: "GameModelId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
