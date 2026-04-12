namespace ClinicManagement.Persistence.Seeders;

public class SeedOptions
{
    public const string Section = "Seed";

    /// <summary>
    /// When false, demo users (Owner, Doctor, Receptionist, SuperAdmin) are not seeded.
    /// Set to false in appsettings.Production.json when you no longer need demo accounts.
    /// </summary>
    public bool SeedDemoUsers { get; init; } = true;

    public SeedUserOptions SuperAdmin { get; init; } = new()
    {
        Email    = "superadmin@clinic.com",
        Password = "SuperAdmin123!"
    };

    public SeedUserOptions ClinicOwner { get; init; } = new()
    {
        Email    = "owner@clinic.com",
        Password = "ClinicOwner123!"
    };

    public SeedUserOptions Doctor { get; init; } = new()
    {
        Email    = "doctor@clinic.com",
        Password = "Doctor123!"
    };

    public SeedUserOptions Receptionist { get; init; } = new()
    {
        Email    = "receptionist@clinic.com",
        Password = "Receptionist123!"
    };
}

public class SeedUserOptions
{
    public string Email    { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
