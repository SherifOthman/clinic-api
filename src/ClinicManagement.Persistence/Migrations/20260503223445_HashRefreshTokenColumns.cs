using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokenColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshToken",
                newName: "TokenHash");

            migrationBuilder.RenameColumn(
                name: "ReplacedByToken",
                table: "RefreshToken",
                newName: "ReplacedByTokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshToken",
                newName: "IX_RefreshToken_TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                table: "RefreshToken",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "ReplacedByTokenHash",
                table: "RefreshToken",
                newName: "ReplacedByToken");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                newName: "IX_RefreshToken_Token");
        }
    }
}
