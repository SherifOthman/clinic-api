using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogEmailAndUserAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLogin",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "UserLogin",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
