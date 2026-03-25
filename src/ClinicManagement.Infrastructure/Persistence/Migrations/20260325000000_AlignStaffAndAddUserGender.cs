using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignStaffAndAddUserGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop extra columns from Staff that no longer exist in the entity
            migrationBuilder.DropColumn(name: "HireDate", table: "Staff");
            migrationBuilder.DropColumn(name: "IsPrimaryClinic", table: "Staff");
            migrationBuilder.DropColumn(name: "Status", table: "Staff");
            migrationBuilder.DropColumn(name: "StatusChangedAt", table: "Staff");
            migrationBuilder.DropColumn(name: "StatusChangedBy", table: "Staff");
            migrationBuilder.DropColumn(name: "StatusReason", table: "Staff");
            migrationBuilder.DropColumn(name: "TerminationDate", table: "Staff");

            // Add IsMale to Users
            migrationBuilder.AddColumn<bool>(
                name: "IsMale",
                table: "Users",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsMale", table: "Users");

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Staff",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryClinic",
                table: "Staff",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Staff",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                table: "Staff",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusChangedBy",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "Staff",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                table: "Staff",
                type: "datetime2",
                nullable: true);
        }
    }
}
