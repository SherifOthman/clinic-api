using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToTenantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RadiologyTest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RadiologyTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RadiologyTest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "RadiologyTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RadiologyTest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RadiologyTest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "RadiologyTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LabTest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LabTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LabTest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "LabTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LabTest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LabTest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LabTest",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClinicMedication",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ClinicMedication",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ClinicMedication",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "ClinicMedication",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClinicMedication",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClinicMedication",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ClinicMedication",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RadiologyTest");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LabTest");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClinicMedication");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ClinicMedication");
        }
    }
}
