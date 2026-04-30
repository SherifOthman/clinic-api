using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Default permissions assigned to each role when a clinic member is created.
/// The clinic owner can add or remove permissions per member after creation.
/// </summary>
public static class DefaultPermissions
{
    public static readonly IReadOnlyList<Permission> Owner =
    [
        Permission.ViewPatients,
        Permission.CreatePatient,
        Permission.EditPatient,
        Permission.DeletePatient,
        Permission.ViewStaff,
        Permission.InviteStaff,
        Permission.ManageStaffStatus,
        Permission.ViewBranches,
        Permission.ManageBranches,
        Permission.ManageSchedule,
        Permission.ManageVisitTypes,
        Permission.ViewAppointments,
        Permission.ManageAppointments,
        Permission.ViewInvoices,
        Permission.ManageInvoices,
    ];

    public static readonly IReadOnlyList<Permission> Doctor =
    [
        Permission.ViewPatients,
        Permission.CreatePatient,
        Permission.EditPatient,
        Permission.ViewBranches,       // needed to load branches in ScheduleTab
        Permission.ManageSchedule,
        Permission.ManageVisitTypes,
        Permission.ViewAppointments,
        Permission.ManageAppointments,
    ];

    public static readonly IReadOnlyList<Permission> Receptionist =
    [
        Permission.ViewPatients,
        Permission.CreatePatient,
        Permission.EditPatient,
        Permission.ViewBranches,
        Permission.ViewAppointments,
        Permission.ManageAppointments,
        Permission.ViewInvoices,
    ];

    public static readonly IReadOnlyList<Permission> Nurse =
    [
        Permission.ViewPatients,
        Permission.ViewAppointments,
    ];

    public static IReadOnlyList<Permission> ForRole(ClinicMemberRole role) => role switch
    {
        ClinicMemberRole.Owner        => Owner,
        ClinicMemberRole.Doctor       => Doctor,
        ClinicMemberRole.Receptionist => Receptionist,
        ClinicMemberRole.Nurse        => Nurse,
        _                             => [],
    };
}
