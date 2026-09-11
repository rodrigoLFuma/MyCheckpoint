using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{

    public partial class AddTitleToUserGame : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "user_games",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "user_games");
        }
    }
}
