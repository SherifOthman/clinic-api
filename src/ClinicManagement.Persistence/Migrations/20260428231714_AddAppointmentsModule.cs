using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentType",
                table: "DoctorInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QueueCounters",
                columns: table => new
                {
                    DoctorInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    LastValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueCounters", x => new { x.DoctorInfoId, x.Date });
                    table.ForeignKey(
                        name: "FK_QueueCounters_DoctorInfo_DoctorInfoId",
                        column: x => x.DoctorInfoId,
                        principalTable: "DoctorInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueCounters");

            migrationBuilder.DropColumn(
                name: "AppointmentType",
                table: "DoctorInfo");
        }
    }
}
