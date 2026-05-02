using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePersonEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicMember_Person_PersonId",
                table: "ClinicMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Patient_Person_PersonId",
                table: "Patient");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Person_PersonId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Users_PersonId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Patient_PersonId",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_PersonId_ClinicId",
                table: "ClinicMember");

            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_UserId",
                table: "ClinicMember");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "ClinicMember");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Patient",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Patient",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Patient",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // ── Data migration: copy personal data from Person to Users and Patient ──
            migrationBuilder.Sql(@"
                UPDATE u
                SET u.FullName = p.FullName,
                    u.Gender   = p.Gender,
                    u.ProfileImageUrl = p.ProfileImageUrl
                FROM Users u
                INNER JOIN Person p ON u.PersonId = p.Id
                WHERE u.PersonId IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE pt
                SET pt.FullName     = p.FullName,
                    pt.Gender       = p.Gender,
                    pt.DateOfBirth  = p.DateOfBirth
                FROM Patient pt
                INNER JOIN Person p ON pt.PersonId = p.Id
                WHERE pt.PersonId IS NOT NULL;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Patient_Gender",
                table: "Patient",
                sql: "[Gender] IN ('Male', 'Female')");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_UserId_ClinicId",
                table: "ClinicMember",
                columns: new[] { "UserId", "ClinicId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Patient_Gender",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_UserId_ClinicId",
                table: "ClinicMember");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Patient");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "Patient",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ClinicMember",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "ClinicMember",                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                    table.CheckConstraint("CK_Person_Gender", "[Gender] IN ('Male', 'Female')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonId",
                table: "Users",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PersonId",
                table: "Patient",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_PersonId_ClinicId",
                table: "ClinicMember",
                columns: new[] { "PersonId", "ClinicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_UserId",
                table: "ClinicMember",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicMember_Person_PersonId",
                table: "ClinicMember",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_Person_PersonId",
                table: "Patient",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Person_PersonId",
                table: "Users",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
