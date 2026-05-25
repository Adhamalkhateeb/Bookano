using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookano.Infrastructure.Persistence
{
    /// <inheritdoc />
    public partial class extendUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                )
            );

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedOnUtc",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreatedOnUtc", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "FullName", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "LastUpdatedOnUtc", table: "AspNetUsers");
        }
    }
}
