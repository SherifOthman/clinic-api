namespace ClinicManagement.API.Common.Constants;

/// <summary>
/// Defines all role names used in the application
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClinicOwner = "ClinicOwner";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";

    public static readonly string[] AllRoles = 
    {
        SuperAdmin,
        ClinicOwner,
        Doctor,
        Receptionist
    };
}
