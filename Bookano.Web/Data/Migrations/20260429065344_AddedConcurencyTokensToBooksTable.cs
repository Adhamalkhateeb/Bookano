using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedConcurencyTokensToBooksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Books",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Books",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Books_IdempotencyKey",
                table: "Books",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_IdempotencyKey",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Books");
        }
    }
}
