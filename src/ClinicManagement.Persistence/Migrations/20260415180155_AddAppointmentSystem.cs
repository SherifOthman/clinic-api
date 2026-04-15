using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkingDay_DoctorProfile_DoctorId",
                table: "DoctorWorkingDay");

            migrationBuilder.DropTable(
                name: "DoctorProfile");

            migrationBuilder.AddColumn<Guid>(
                name: "ClinicId1",
                table: "ClinicBranch",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctor_Clinic_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinic",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Doctor_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doctor_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    QueueNumber = table.Column<int>(type: "int", nullable: true),
                    ScheduledTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    VisitTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointment_ClinicBranch_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointment_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointment_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointment_VisitType_VisitTypeId",
                        column: x => x.VisitTypeId,
                        principalTable: "VisitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchVisitType",
                columns: table => new
                {
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchVisitType", x => new { x.ClinicBranchId, x.VisitTypeId });
                    table.ForeignKey(
                        name: "FK_BranchVisitType_ClinicBranch_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchVisitType_VisitType_VisitTypeId",
                        column: x => x.VisitTypeId,
                        principalTable: "VisitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranch_ClinicId1",
                table: "ClinicBranch",
                column: "ClinicId1");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ClinicBranchId_Date_Status",
                table: "Appointment",
                columns: new[] { "ClinicBranchId", "Date", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorId_Date_QueueNumber",
                table: "Appointment",
                columns: new[] { "DoctorId", "Date", "QueueNumber" },
                unique: true,
                filter: "[QueueNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_DoctorId_Date_ScheduledTime",
                table: "Appointment",
                columns: new[] { "DoctorId", "Date", "ScheduledTime" },
                unique: true,
                filter: "[ScheduledTime] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientId_Date",
                table: "Appointment",
                columns: new[] { "PatientId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_VisitTypeId",
                table: "Appointment",
                column: "VisitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchVisitType_VisitTypeId",
                table: "BranchVisitType",
                column: "VisitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_ClinicId",
                table: "Doctor",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_SpecializationId",
                table: "Doctor",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_StaffId",
                table: "Doctor",
                column: "StaffId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicBranch_Clinic_ClinicId1",
                table: "ClinicBranch",
                column: "ClinicId1",
                principalTable: "Clinic",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkingDay_Doctor_DoctorId",
                table: "DoctorWorkingDay",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicBranch_Clinic_ClinicId1",
                table: "ClinicBranch");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkingDay_Doctor_DoctorId",
                table: "DoctorWorkingDay");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "BranchVisitType");

            migrationBuilder.DropTable(
                name: "Doctor");

            migrationBuilder.DropTable(
                name: "VisitType");

            migrationBuilder.DropIndex(
                name: "IX_ClinicBranch_ClinicId1",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "ClinicId1",
                table: "ClinicBranch");

            migrationBuilder.CreateTable(
                name: "DoctorProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorProfile_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorProfile_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfile_SpecializationId",
                table: "DoctorProfile",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfile_StaffId",
                table: "DoctorProfile",
                column: "StaffId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkingDay_DoctorProfile_DoctorId",
                table: "DoctorWorkingDay",
                column: "DoctorId",
                principalTable: "DoctorProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
