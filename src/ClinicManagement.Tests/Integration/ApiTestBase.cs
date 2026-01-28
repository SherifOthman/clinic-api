using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Integration;

public class ApiTestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly UserManager<User> _userManager;
    protected readonly RoleManager<IdentityRole<int>> _roleManager;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly Mock<ICurrentUserService> _currentUserServiceMock;
    protected readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public ApiTestBase()
    {
        var services = new ServiceCollection();
        
        // Add mock services first
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        services.AddScoped<ICurrentUserService>(_ => _currentUserServiceMock.Object);
        services.AddScoped<IDateTimeProvider>(_ => _dateTimeProviderMock.Object);

        // Add in-memory database
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        });

        // Add logging services
        services.AddLogging();

        // Add Identity services
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Override in derived classes to add specific test data
    }

    protected async Task<User> CreateTestUserAsync(string email, string password, string role)
    {
        // Create role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }

        var user = new User
        {
            UserName = email.Split('@')[0],
            Email = email,
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    protected async Task<Clinic> CreateTestClinicAsync(string name, int subscriptionPlanId)
    {
        var clinic = new Clinic
        {
            Name = name,
            SubscriptionPlanId = subscriptionPlanId
        };

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();
        return clinic;
    }

    protected async Task<SubscriptionPlan> CreateTestSubscriptionPlanAsync(string name, decimal price)
    {
        var plan = new SubscriptionPlan
        {
            Name = name,
            Description = $"Test plan: {name}",
            Price = price,
            DurationDays = 30,
            MaxUsers = 10,
            MaxPatients = 100,
            IsActive = true
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.GetService<IServiceScope>()?.Dispose();
    }
}