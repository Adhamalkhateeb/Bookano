using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Subscribers_subscriberId",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "subscriberId",
                table: "Rentals",
                newName: "SubscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_subscriberId",
                table: "Rentals",
                newName: "IX_Rentals_SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Subscribers_SubscriberId",
                table: "Rentals",
                column: "SubscriberId",
                principalTable: "Subscribers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Subscribers_SubscriberId",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "SubscriberId",
                table: "Rentals",
                newName: "subscriberId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_SubscriberId",
                table: "Rentals",
                newName: "IX_Rentals_subscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Subscribers_subscriberId",
                table: "Rentals",
                column: "subscriberId",
                principalTable: "Subscribers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
