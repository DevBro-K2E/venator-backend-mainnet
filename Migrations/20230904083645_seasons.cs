using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsometricShooterWebApp.Migrations
{
    public partial class seasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SeasonOffset = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeasonRewards",
                columns: table => new
                {
                    EndOffset = table.Column<int>(type: "integer", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonRewards", x => new { x.SeasonId, x.EndOffset });
                    table.ForeignKey(
                        name: "FK_SeasonRewards_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonUserRewards",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Offset = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<double>(type: "double precision", nullable: false),
                    Taken = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonUserRewards", x => new { x.SeasonId, x.UserId, x.Date });
                    table.ForeignKey(
                        name: "FK_SeasonUserRewards_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonUserRewards_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonUsers", x => new { x.SeasonId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SeasonUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonUsers_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonUserRewards_UserId",
                table: "SeasonUserRewards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonUsers_UserId",
                table: "SeasonUsers",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonRewards");

            migrationBuilder.DropTable(
                name: "SeasonUserRewards");

            migrationBuilder.DropTable(
                name: "SeasonUsers");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
