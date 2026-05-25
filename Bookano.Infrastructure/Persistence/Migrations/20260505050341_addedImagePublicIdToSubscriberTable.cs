using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Infrastructure.Persistence
{
    /// <inheritdoc />
    public partial class addedImagePublicIdToSubscriberTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Subscribers");
        }
    }
}
