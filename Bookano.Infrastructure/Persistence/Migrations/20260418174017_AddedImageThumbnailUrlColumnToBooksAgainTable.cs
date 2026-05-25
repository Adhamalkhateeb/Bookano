using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Infrastructure.Persistence
{
    /// <inheritdoc />
    public partial class AddedImageThumbnailUrlColumnToBooksAgainTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageThumbnailUrl",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageThumbnailUrl",
                table: "Books");
        }
    }
}
