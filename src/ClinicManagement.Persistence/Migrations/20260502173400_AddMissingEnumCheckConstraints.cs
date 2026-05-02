using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEnumCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_DoctorSession_DelayHandling",
                table: "DoctorSessions",
                sql: "[DelayHandling] IS NULL OR [DelayHandling] IN ('AutoShift', 'MarkMissed', 'Manual')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClinicMemberPermission_Permission",
                table: "ClinicMemberPermission",
                sql: "[Permission] IN ('ViewPatients','CreatePatient','EditPatient','DeletePatient','ViewStaff','InviteStaff','ManageStaffStatus','ViewBranches','ManageBranches','ManageSchedule','ManageVisitTypes','ViewAppointments','ManageAppointments','ViewInvoices','ManageInvoices')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DoctorSession_DelayHandling",
                table: "DoctorSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClinicMemberPermission_Permission",
                table: "ClinicMemberPermission");
        }
    }
}
