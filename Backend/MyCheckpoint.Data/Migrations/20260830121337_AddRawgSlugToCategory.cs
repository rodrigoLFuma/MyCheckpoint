using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCheckpoint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRawgSlugToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawgSlug",
                table: "categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawgSlug",
                table: "categories");
        }
    }
}
