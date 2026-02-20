using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Seeders;

public class ClinicOwnerSeedService
{
    private readonly string _connectionString;
    private readonly ILogger<ClinicOwnerSeedService> _logger;

    public ClinicOwnerSeedService(
        IConfiguration configuration,
        ILogger<ClinicOwnerSeedService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task SeedClinicOwnerDataAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if owner user exists
            var ownerUser = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = "owner@clinic.com" });

            if (ownerUser == null)
            {
                _logger.LogInformation("Clinic Owner user not found, skipping clinic seed");
                return;
            }

            // Check if clinic already exists
            var existingClinicId = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT Id FROM Clinics WHERE OwnerUserId = @OwnerUserId",
                new { OwnerUserId = ownerUser.Id });

            int clinicId;

            if (existingClinicId.HasValue)
            {
                _logger.LogInformation("Demo clinic already exists");
                clinicId = existingClinicId.Value;
            }
            else
            {
                // Get Basic Plan ID
                var basicPlanId = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT Id FROM SubscriptionPlans WHERE Name = @Name",
                    new { Name = "Basic" });

                if (basicPlanId == 0)
                {
                    _logger.LogError("Basic Plan not found");
                    return;
                }

                // Create clinic
                clinicId = await connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO Clinics (Name, OwnerUserId, SubscriptionPlanId, OnboardingCompleted, IsActive, CreatedAt, IsDeleted)
                    VALUES (@Name, @OwnerUserId, @SubscriptionPlanId, @OnboardingCompleted, @IsActive, @CreatedAt, @IsDeleted);
                    SELECT CAST(SCOPE_IDENTITY() as int)",
                    new
                    {
                        Name = "Demo Medical Clinic",
                        OwnerUserId = ownerUser.Id,
                        SubscriptionPlanId = basicPlanId,
                        OnboardingCompleted = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });

                _logger.LogInformation("Created demo clinic for owner@clinic.com");
            }

            // Check if staff record already exists
            var existingStaff = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT Id FROM Staff WHERE UserId = @UserId AND ClinicId = @ClinicId AND IsActive = 1",
                new { UserId = ownerUser.Id, ClinicId = clinicId });

            if (existingStaff.HasValue)
            {
                _logger.LogInformation("Staff record already exists for owner@clinic.com");
                return;
            }

            // Create staff record
            await connection.ExecuteAsync(@"
                INSERT INTO Staff (ClinicId, UserId, IsActive, HireDate, CreatedAt)
                VALUES (@ClinicId, @UserId, @IsActive, @HireDate, @CreatedAt)",
                new
                {
                    ClinicId = clinicId,
                    UserId = ownerUser.Id,
                    IsActive = true,
                    HireDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

            _logger.LogInformation("Created staff record for owner@clinic.com");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding clinic owner data");
        }
    }
}
