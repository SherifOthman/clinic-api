using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixStaffBranchRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffBranches_ClinicBranches_ClinicBranchId1",
                table: "StaffBranches");

            migrationBuilder.DropIndex(
                name: "IX_StaffBranches_ClinicBranchId1",
                table: "StaffBranches");

            migrationBuilder.DropColumn(
                name: "ClinicBranchId1",
                table: "StaffBranches");

            migrationBuilder.AlterColumn<decimal>(
                name: "DecimalValue",
                table: "MedicalVisitMeasurements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClinicBranchId1",
                table: "StaffBranches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DecimalValue",
                table: "MedicalVisitMeasurements",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffBranches_ClinicBranchId1",
                table: "StaffBranches",
                column: "ClinicBranchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffBranches_ClinicBranches_ClinicBranchId1",
                table: "StaffBranches",
                column: "ClinicBranchId1",
                principalTable: "ClinicBranches",
                principalColumn: "Id");
        }
    }
}
