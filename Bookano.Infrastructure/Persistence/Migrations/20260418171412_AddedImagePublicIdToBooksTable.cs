using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Infrastructure.Persistence
{
    /// <inheritdoc />
    public partial class AddedImagePublicIdToBooksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageThumbnailUrl",
                table: "Books",
                newName: "ImagePublicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePublicId",
                table: "Books",
                newName: "ImageThumbnailUrl");
        }
    }
}
