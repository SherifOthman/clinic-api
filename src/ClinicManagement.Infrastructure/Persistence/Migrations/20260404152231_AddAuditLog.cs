using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
