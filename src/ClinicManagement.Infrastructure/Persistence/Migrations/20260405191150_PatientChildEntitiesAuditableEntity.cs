using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientChildEntitiesAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientPhones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientPhones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PatientPhones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientPhones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientPhones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientChronicDiseases",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientChronicDiseases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PatientChronicDiseases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientChronicDiseases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientChronicDiseases",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientPhones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientPhones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PatientPhones");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientPhones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientPhones");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientChronicDiseases");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientChronicDiseases");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PatientChronicDiseases");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientChronicDiseases");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientChronicDiseases");
        }
    }
}
