using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClinicIdFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicineDispensing_MedicalVisit_VisitId",
                table: "MedicineDispensing");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineDispensing_Prescription_PrescriptionId",
                table: "MedicineDispensing");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineDispensing_Users_DispensedByUserId",
                table: "MedicineDispensing");

            migrationBuilder.DropIndex(
                name: "IX_MedicineDispensing_DispensedByUserId",
                table: "MedicineDispensing");

            migrationBuilder.DropIndex(
                name: "IX_MedicineDispensing_PrescriptionId",
                table: "MedicineDispensing");

            migrationBuilder.DropIndex(
                name: "IX_MedicineDispensing_VisitId",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "PrescriptionItemId",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "MedicineDispensing");

            migrationBuilder.DropColumn(
                name: "IsAbnormal",
                table: "LabTestOrder");

            migrationBuilder.RenameColumn(
                name: "ResultsText",
                table: "LabTestOrder",
                newName: "ResultNotes");

            migrationBuilder.RenameColumn(
                name: "ResultsFilePath",
                table: "LabTestOrder",
                newName: "ResultFilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultNotes",
                table: "LabTestOrder",
                newName: "ResultsText");

            migrationBuilder.RenameColumn(
                name: "ResultFilePath",
                table: "LabTestOrder",
                newName: "ResultsFilePath");

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                table: "MedicineDispensing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionItemId",
                table: "MedicineDispensing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "MedicineDispensing",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "MedicineDispensing",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VisitId",
                table: "MedicineDispensing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAbnormal",
                table: "LabTestOrder",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_DispensedByUserId",
                table: "MedicineDispensing",
                column: "DispensedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_PrescriptionId",
                table: "MedicineDispensing",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_VisitId",
                table: "MedicineDispensing",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineDispensing_MedicalVisit_VisitId",
                table: "MedicineDispensing",
                column: "VisitId",
                principalTable: "MedicalVisit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineDispensing_Prescription_PrescriptionId",
                table: "MedicineDispensing",
                column: "PrescriptionId",
                principalTable: "Prescription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineDispensing_Users_DispensedByUserId",
                table: "MedicineDispensing",
                column: "DispensedByUserId",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
