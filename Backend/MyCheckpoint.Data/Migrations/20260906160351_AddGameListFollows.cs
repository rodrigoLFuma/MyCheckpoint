using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{

    public partial class AddGameListFollows : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_list_follows",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GameListId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_list_follows", x => new { x.UserId, x.GameListId });
                    table.ForeignKey(
                        name: "FK_game_list_follows_game_lists_GameListId",
                        column: x => x.GameListId,
                        principalTable: "game_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_list_follows_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_list_follows_GameListId",
                table: "game_list_follows",
                column: "GameListId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_list_follows");
        }
    }
}
