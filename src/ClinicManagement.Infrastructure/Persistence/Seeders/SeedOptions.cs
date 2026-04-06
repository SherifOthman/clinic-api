namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Configuration for demo/dev seed user credentials.
/// Override in appsettings.Development.json or user secrets.
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
