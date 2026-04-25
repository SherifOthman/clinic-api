using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceFirstLastNameWithFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Person");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Person",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Person");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Person",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Person",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
