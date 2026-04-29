using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSessionAndDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultVisitDurationMinutes",
                table: "DoctorInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Appointment",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitDurationMinutes",
                table: "Appointment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DoctorSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckedInAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CheckedOutAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ScheduledStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    DelayHandling = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSessions_ClinicBranch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "ClinicBranch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DoctorSessions_DoctorInfo_DoctorInfoId",
                        column: x => x.DoctorInfoId,
                        principalTable: "DoctorInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSessions_BranchId",
                table: "DoctorSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSessions_DoctorInfoId_BranchId_Date",
                table: "DoctorSessions",
                columns: new[] { "DoctorInfoId", "BranchId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorSessions");

            migrationBuilder.DropColumn(
                name: "DefaultVisitDurationMinutes",
                table: "DoctorInfo");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "VisitDurationMinutes",
                table: "Appointment");
        }
    }
}
