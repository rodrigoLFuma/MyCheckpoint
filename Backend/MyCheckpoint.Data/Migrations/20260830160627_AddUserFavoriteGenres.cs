using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteGenres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_favorite_genres",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_favorite_genres", x => new { x.UserId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_user_favorite_genres_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_favorite_genres_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_favorite_genres_CategoryId",
                table: "user_favorite_genres",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_favorite_genres");
        }
    }
}
