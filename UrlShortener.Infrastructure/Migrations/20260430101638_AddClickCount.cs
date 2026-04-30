using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClickCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "ShortenedUrls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "ShortenedUrls");
        }
    }
}
