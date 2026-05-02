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
            // These objects only exist if the Person table was previously created.
            // On fresh databases the Person entity was never applied, so skip safely.
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_ClinicMember_Person_PersonId', 'F') IS NOT NULL
                    ALTER TABLE [ClinicMember] DROP CONSTRAINT [FK_ClinicMember_Person_PersonId];
                IF OBJECT_ID('FK_Patient_Person_PersonId', 'F') IS NOT NULL
                    ALTER TABLE [Patient] DROP CONSTRAINT [FK_Patient_Person_PersonId];
                IF OBJECT_ID('FK_Users_Person_PersonId', 'F') IS NOT NULL
                    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Person_PersonId];
                IF OBJECT_ID('Person', 'U') IS NOT NULL
                    DROP TABLE [Person];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_PersonId' AND object_id = OBJECT_ID('Users'))
                    DROP INDEX [IX_Users_PersonId] ON [Users];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Patient_PersonId' AND object_id = OBJECT_ID('Patient'))
                    DROP INDEX [IX_Patient_PersonId] ON [Patient];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ClinicMember_PersonId_ClinicId' AND object_id = OBJECT_ID('ClinicMember'))
                    DROP INDEX [IX_ClinicMember_PersonId_ClinicId] ON [ClinicMember];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ClinicMember_UserId' AND object_id = OBJECT_ID('ClinicMember'))
                    DROP INDEX [IX_ClinicMember_UserId] ON [ClinicMember];
                IF COL_LENGTH('Users', 'PersonId') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [PersonId];
                IF COL_LENGTH('Patient', 'PersonId') IS NOT NULL
                    ALTER TABLE [Patient] DROP COLUMN [PersonId];
                IF COL_LENGTH('ClinicMember', 'PersonId') IS NOT NULL
                    ALTER TABLE [ClinicMember] DROP COLUMN [PersonId];
            ");
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
            // Only runs if Person table exists (may have been dropped already)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Person', 'U') IS NOT NULL
                BEGIN
                    UPDATE u
                    SET u.FullName = p.FullName,
                        u.Gender   = p.Gender,
                        u.ProfileImageUrl = p.ProfileImageUrl
                    FROM Users u
                    INNER JOIN Person p ON u.PersonId = p.Id
                    WHERE u.PersonId IS NOT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID('Person', 'U') IS NOT NULL
                BEGIN
                    UPDATE pt
                    SET pt.FullName     = p.FullName,
                        pt.Gender       = p.Gender,
                        pt.DateOfBirth  = p.DateOfBirth
                    FROM Patient pt
                    INNER JOIN Person p ON pt.PersonId = p.Id
                    WHERE pt.PersonId IS NOT NULL;
                END
            ");

            // Fix any invalid Gender values before adding check constraints
            migrationBuilder.Sql("UPDATE [Patient] SET [Gender] = 'Male' WHERE [Gender] NOT IN ('Male', 'Female') OR [Gender] IS NULL OR [Gender] = '';");
            migrationBuilder.Sql("UPDATE [Users]   SET [Gender] = 'Male' WHERE [Gender] NOT IN ('Male', 'Female') OR [Gender] IS NULL OR [Gender] = '';");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Patient_Gender') ALTER TABLE [Patient] ADD CONSTRAINT [CK_Patient_Gender] CHECK ([Gender] IN ('Male', 'Female'));");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_User_Gender')    ALTER TABLE [Users]   ADD CONSTRAINT [CK_User_Gender]   CHECK ([Gender] IN ('Male', 'Female'));");

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

            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Gender",
                table: "Users");

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
