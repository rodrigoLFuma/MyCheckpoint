using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{

    public partial class AddRawgFieldsToGame : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Developer",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Metacritic",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawgId",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_RawgId",
                table: "games",
                column: "RawgId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_games_RawgId",
                table: "games");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "games");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "games");

            migrationBuilder.DropColumn(
                name: "Developer",
                table: "games");

            migrationBuilder.DropColumn(
                name: "Metacritic",
                table: "games");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "games");

            migrationBuilder.DropColumn(
                name: "RawgId",
                table: "games");
        }
    }
}
