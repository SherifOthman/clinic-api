namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Credentials for the four required system users.
/// Defaults are defined here in code — no JSON config needed.
/// Override per environment via environment variables:
///   Seed__SuperAdmin__Password=...
///   Seed__ClinicOwner__Password=...
///   etc.
/// </summary>
public class SeedOptions
{
    public const string Section = "Seed";

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
