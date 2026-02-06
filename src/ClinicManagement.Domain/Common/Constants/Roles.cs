namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Defines all role names used in the application
/// </summary>
public static class Roles
{
    public const string ClinicOwner = "ClinicOwner";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
    public const string Nurse = "Nurse";
    public const string Pharmacist = "Pharmacist";
    public const string LabTechnician = "LabTechnician";
    public const string Accountant = "Accountant";

    public static readonly string[] AllRoles = 
    {
        ClinicOwner,
        Doctor,
        Receptionist,
        Nurse,
        Pharmacist,
        LabTechnician,
        Accountant
    };
}
