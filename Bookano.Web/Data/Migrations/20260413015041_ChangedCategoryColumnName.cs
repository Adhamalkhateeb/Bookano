using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedCategoryColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedOnUtc",
                table: "Categories",
                newName: "LastUpdatedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedOnUtc",
                table: "Categories",
                newName: "UpdatedOnUtc");
        }
    }
}
