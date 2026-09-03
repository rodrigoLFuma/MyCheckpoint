using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "review_likes",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserGameId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_likes", x => new { x.UserId, x.UserGameId });
                    table.ForeignKey(
                        name: "FK_review_likes_user_games_UserGameId",
                        column: x => x.UserGameId,
                        principalTable: "user_games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_review_likes_UserGameId",
                table: "review_likes",
                column: "UserGameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_likes");
        }
    }
}
