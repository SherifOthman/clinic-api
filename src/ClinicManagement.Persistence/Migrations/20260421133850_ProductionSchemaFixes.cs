using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductionSchemaFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patient_PatientCode",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "BooleanValue",
                table: "MedicalVisitMeasurement");

            migrationBuilder.DropColumn(
                name: "DateTimeValue",
                table: "MedicalVisitMeasurement");

            migrationBuilder.DropColumn(
                name: "DecimalValue",
                table: "MedicalVisitMeasurement");

            migrationBuilder.DropColumn(
                name: "IntValue",
                table: "MedicalVisitMeasurement");

            migrationBuilder.DropColumn(
                name: "StringValue",
                table: "MedicalVisitMeasurement");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicalVisitMeasurement",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValuesJson",
                table: "MedicalVisitMeasurement",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.CreateTable(
                name: "PatientCounters",
                columns: table => new
                {
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientCounters", x => x.ClinicId);
                    table.ForeignKey(
                        name: "FK_PatientCounters_Clinic_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_ClinicId_PatientCode",
                table: "Patient",
                columns: new[] { "ClinicId", "PatientCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitMeasurement_MedicalVisitId_MeasurementAttributeId",
                table: "MedicalVisitMeasurement",
                columns: new[] { "MedicalVisitId", "MeasurementAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItem_InvoiceId",
                table: "InvoiceItem",
                column: "InvoiceId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InvoiceItem_ExactlyOneSource",
                table: "InvoiceItem",
                sql: "(CASE WHEN [MedicalServiceId]     IS NOT NULL THEN 1 ELSE 0 END +\r\n                   CASE WHEN [MedicineId]            IS NOT NULL THEN 1 ELSE 0 END +\r\n                   CASE WHEN [MedicalSupplyId]       IS NOT NULL THEN 1 ELSE 0 END +\r\n                   CASE WHEN [MedicineDispensingId]  IS NOT NULL THEN 1 ELSE 0 END +\r\n                   CASE WHEN [LabTestOrderId]        IS NOT NULL THEN 1 ELSE 0 END +\r\n                   CASE WHEN [RadiologyOrderId]      IS NOT NULL THEN 1 ELSE 0 END) = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientCounters");

            migrationBuilder.DropIndex(
                name: "IX_Patient_ClinicId_PatientCode",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_MedicalVisitMeasurement_MedicalVisitId_MeasurementAttributeId",
                table: "MedicalVisitMeasurement");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItem_InvoiceId",
                table: "InvoiceItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InvoiceItem_ExactlyOneSource",
                table: "InvoiceItem");

            migrationBuilder.DropColumn(
                name: "ValuesJson",
                table: "MedicalVisitMeasurement");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicalVisitMeasurement",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BooleanValue",
                table: "MedicalVisitMeasurement",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateTimeValue",
                table: "MedicalVisitMeasurement",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DecimalValue",
                table: "MedicalVisitMeasurement",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntValue",
                table: "MedicalVisitMeasurement",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StringValue",
                table: "MedicalVisitMeasurement",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PatientCode",
                table: "Patient",
                column: "PatientCode",
                unique: true);
        }
    }
}
