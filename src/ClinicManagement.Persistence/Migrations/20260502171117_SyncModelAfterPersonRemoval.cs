using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterPersonRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_UserId_ClinicId",
                table: "ClinicMember");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ClinicMember",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_User_Gender') ALTER TABLE [Users] ADD CONSTRAINT [CK_User_Gender] CHECK ([Gender] IN ('Male', 'Female'));");

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
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_User_Gender') ALTER TABLE [Users] DROP CONSTRAINT [CK_User_Gender]");

            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_UserId_ClinicId",
                table: "ClinicMember");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ClinicMember",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_UserId_ClinicId",
                table: "ClinicMember",
                columns: new[] { "UserId", "ClinicId" },
                unique: true);
        }
    }
}
