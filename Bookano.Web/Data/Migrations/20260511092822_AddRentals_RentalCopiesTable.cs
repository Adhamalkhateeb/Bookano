using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRentals_RentalCopiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subscriberId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PenaltyPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rentals_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rentals_AspNetUsers_LastUpdatedById",
                        column: x => x.LastUpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rentals_Subscribers_subscriberId",
                        column: x => x.subscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalCopies",
                columns: table => new
                {
                    RentalId = table.Column<int>(type: "int", nullable: false),
                    CopyId = table.Column<int>(type: "int", nullable: false),
                    RentalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExtendedOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalCopies", x => new { x.RentalId, x.CopyId });
                    table.ForeignKey(
                        name: "FK_RentalCopies_BookCopies_CopyId",
                        column: x => x.CopyId,
                        principalTable: "BookCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalCopies_Rentals_RentalId",
                        column: x => x.RentalId,
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalCopies_CopyId",
                table: "RentalCopies",
                column: "CopyId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CreatedById",
                table: "Rentals",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_LastUpdatedById",
                table: "Rentals",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_subscriberId",
                table: "Rentals",
                column: "subscriberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalCopies");

            migrationBuilder.DropTable(
                name: "Rentals");
        }
    }
}
