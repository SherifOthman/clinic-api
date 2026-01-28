namespace ClinicManagement.Domain.Common.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ClinicOwner = "ClinicOwner";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";

    public const string AllStaff = Admin + "," + ClinicOwner + "," + Doctor + "," + Receptionist;
    public const string Management = ClinicOwner;
    public const string PatientManagement = Admin + "," + ClinicOwner + "," + Receptionist;
    public const string StaffManagement = Admin + "," + ClinicOwner;
    public const string MedicalStaff = Admin + "," + ClinicOwner + "," + Doctor;
    public const string SystemAdmin = Admin;

    public const string AdminAndManagement = Admin + "," + ClinicOwner;
}