using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeForoeignKeyNameOfBookCopiesInRentalsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalCopies_BookCopies_CopyId",
                table: "RentalCopies");

            migrationBuilder.RenameColumn(
                name: "CopyId",
                table: "RentalCopies",
                newName: "BookCopyId");

            migrationBuilder.RenameIndex(
                name: "IX_RentalCopies_CopyId",
                table: "RentalCopies",
                newName: "IX_RentalCopies_BookCopyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalCopies_BookCopies_BookCopyId",
                table: "RentalCopies",
                column: "BookCopyId",
                principalTable: "BookCopies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalCopies_BookCopies_BookCopyId",
                table: "RentalCopies");

            migrationBuilder.RenameColumn(
                name: "BookCopyId",
                table: "RentalCopies",
                newName: "CopyId");

            migrationBuilder.RenameIndex(
                name: "IX_RentalCopies_BookCopyId",
                table: "RentalCopies",
                newName: "IX_RentalCopies_CopyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalCopies_BookCopies_CopyId",
                table: "RentalCopies",
                column: "CopyId",
                principalTable: "BookCopies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
