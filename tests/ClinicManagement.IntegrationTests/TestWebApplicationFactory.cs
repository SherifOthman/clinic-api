using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClinicManagement.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Configures in-memory database and test services
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "TestDatabase_" + Guid.NewGuid();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureTestServices(services =>
        {
            // Add in-memory database with a unique name per factory instance
            // Configure to ignore transaction warnings since InMemory doesn't support transactions
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName)
                       .ConfigureWarnings(warnings => warnings.Ignore(
                           Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Override DateTimeProvider for consistent testing
            services.RemoveAll(typeof(DateTimeProvider));
            services.AddSingleton<DateTimeProvider>();
        });
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        
        // Seed test data after host is created
        using var scope = host.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        SeedRolesAsync(roleManager).GetAwaiter().GetResult();
        SeedReferenceDataAsync(db).GetAwaiter().GetResult();
        
        return host;
    }
    
    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "SuperAdmin", "ClinicOwner", "Doctor", "Receptionist" };
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                // Explicitly set NormalizedName for InMemory database
                var role = new IdentityRole<Guid> 
                { 
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
    
    private static async Task SeedReferenceDataAsync(ApplicationDbContext db)
    {
        // Seed subscription plans if not exists
        if (!await db.SubscriptionPlans.AnyAsync())
        {
            var basicPlan = new SubscriptionPlan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Basic Plan",
                Description = "Basic subscription for testing",
                MonthlyFee = 99.99m,
                YearlyFee = 999.99m,
                SetupFee = 0m,
                MaxBranches = 1,
                MaxStaff = 8,
                MaxPatientsPerMonth = 100,
                MaxAppointmentsPerMonth = 500,
                MaxInvoicesPerMonth = 500,
                StorageLimitGB = 5,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = false,
                HasApiAccess = false,
                HasMultipleBranches = false,
                HasCustomBranding = false,
                HasPrioritySupport = false,
                HasBackupAndRestore = true,
                HasIntegrations = false,
                IsActive = true,
                IsPopular = false,
                DisplayOrder = 1
            };
            
            var proPlan = new SubscriptionPlan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Pro Plan",
                Description = "Professional subscription for testing",
                MonthlyFee = 199.99m,
                YearlyFee = 1999.99m,
                SetupFee = 0m,
                MaxBranches = 5,
                MaxStaff = 30,
                MaxPatientsPerMonth = 1000,
                MaxAppointmentsPerMonth = 5000,
                MaxInvoicesPerMonth = 5000,
                StorageLimitGB = 50,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = true,
                HasApiAccess = true,
                HasMultipleBranches = true,
                HasCustomBranding = true,
                HasPrioritySupport = true,
                HasBackupAndRestore = true,
                HasIntegrations = true,
                IsActive = true,
                IsPopular = true,
                DisplayOrder = 2
            };
            
            db.SubscriptionPlans.AddRange(basicPlan, proPlan);
        }
        
        // Seed specializations if not exists
        if (!await db.Specializations.AnyAsync())
        {
            var generalPractice = new Specialization
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                NameEn = "General Practice",
                NameAr = "الممارسة العامة",
                DescriptionEn = "General medical practice",
                DescriptionAr = "الممارسة الطبية العامة",
                IsActive = true
            };
            
            var pediatrics = new Specialization
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                NameEn = "Pediatrics",
                NameAr = "طب الأطفال",
                DescriptionEn = "Medical care for children",
                DescriptionAr = "الرعاية الطبية للأطفال",
                IsActive = true
            };
            
            db.Specializations.AddRange(generalPractice, pediatrics);
        }
        
        await db.SaveChangesAsync();
    }
    
    /// <summary>
    /// Confirms the email for a test user
    /// </summary>
    public async Task ConfirmEmailAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        var user = await userManager.FindByEmailAsync(email);
        if (user != null && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureDeleted();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        base.Dispose(disposing);
    }
}
