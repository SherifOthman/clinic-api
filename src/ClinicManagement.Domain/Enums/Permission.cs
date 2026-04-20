namespace ClinicManagement.Domain.Enums;

/// <summary>
/// Fine-grained permissions that can be granted or revoked per clinic member.
/// Role defaults are defined in ClinicManagement.Domain.Common.Constants.DefaultPermissions.
/// </summary>
public enum Permission
{
    // ── Patients ──────────────────────────────────────────────────────────────
    ViewPatients,
    CreatePatient,
    EditPatient,
    DeletePatient,

    // ── Staff ─────────────────────────────────────────────────────────────────
    ViewStaff,
    InviteStaff,
    ManageStaffStatus,

    // ── Branches ──────────────────────────────────────────────────────────────
    ViewBranches,
    ManageBranches,

    // ── Schedule & Visit Types ────────────────────────────────────────────────
    ManageSchedule,
    ManageVisitTypes,

    // ── Appointments ──────────────────────────────────────────────────────────
    ViewAppointments,
    ManageAppointments,

    // ── Invoices ──────────────────────────────────────────────────────────────
    ViewInvoices,
    ManageInvoices,
}
