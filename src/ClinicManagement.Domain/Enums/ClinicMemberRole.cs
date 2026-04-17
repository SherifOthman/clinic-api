namespace ClinicManagement.Domain.Enums;

/// <summary>
/// The role a ClinicMember plays within their clinic.
/// Stored on ClinicMember — per-clinic, not global.
/// ASP.NET Identity roles are kept for HTTP-level auth only.
/// Adding a new role: add it here + update IPermissionService only.
/// </summary>
public enum ClinicMemberRole
{
    Owner        = 0,
    Doctor       = 1,
    Receptionist = 2,
    Nurse        = 3,
}
