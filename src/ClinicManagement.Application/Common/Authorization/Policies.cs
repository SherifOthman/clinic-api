namespace ClinicManagement.Application.Common.Authorization;

/// <summary>
/// Authorization policy names used throughout the application.
/// </summary>
public static class Policies
{
    // Role-based policies
    public const string RequireClinicOwner = "RequireClinicOwner";
    public const string RequireDoctor = "RequireDoctor";
    public const string RequireReceptionist = "RequireReceptionist";
    public const string RequireSystemAdmin = "RequireSystemAdmin";
    public const string RequireStaffMember = "RequireStaffMember"; // Doctor, Receptionist, or Nurse
    
    // Resource-based policies
    public const string SameClinic = "SameClinic";
    
    // Combined policies
    public const string ManageStaff = "ManageStaff"; // ClinicOwner + SameClinic
    public const string ManagePatients = "ManagePatients"; // Doctor/Receptionist + SameClinic
    public const string ManageSubscription = "ManageSubscription"; // ClinicOwner only
    public const string ViewReports = "ViewReports"; // ClinicOwner/Doctor + SameClinic
}
