using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Tests.Services;

public class CodeGeneratorServiceTests
{
    [Fact]
    public async Task GenerateInvoiceNumber_ShouldGenerateCorrectFormat()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessor = new TestHttpContextAccessor(Guid.NewGuid());
        var currentUserService = new CurrentUserService(httpContextAccessor);
        var dateTimeProvider = new DateTimeProvider();

        using var context = new ApplicationDbContext(options, currentUserService, dateTimeProvider);
        var service = new CodeGeneratorService(context, currentUserService);

        // Act
        var invoiceNumber = await service.GenerateInvoiceNumberAsync();

        // Assert
        invoiceNumber.Should().MatchRegex(@"^INV-\d{4}-\d{6}$");
        invoiceNumber.Should().Contain(DateTime.UtcNow.Year.ToString());
    }

    [Fact]
    public async Task GeneratePatientNumber_ShouldGenerateCorrectFormat()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessor = new TestHttpContextAccessor(Guid.NewGuid());
        var currentUserService = new CurrentUserService(httpContextAccessor);
        var dateTimeProvider = new DateTimeProvider();

        using var context = new ApplicationDbContext(options, currentUserService, dateTimeProvider);
        var service = new CodeGeneratorService(context, currentUserService);

        // Act
        var patientNumber = await service.GeneratePatientNumberAsync();

        // Assert
        patientNumber.Should().MatchRegex(@"^PAT-\d{4}-\d{6}$");
        patientNumber.Should().Contain(DateTime.UtcNow.Year.ToString());
    }
}

// Test helper class to provide a fake HttpContextAccessor
internal class TestHttpContextAccessor : Microsoft.AspNetCore.Http.IHttpContextAccessor
{
    private readonly Guid _clinicId;

    public TestHttpContextAccessor(Guid clinicId)
    {
        _clinicId = clinicId;
    }

    public Microsoft.AspNetCore.Http.HttpContext? HttpContext
    {
        get
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new System.Security.Claims.Claim("ClinicId", _clinicId.ToString())
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            context.User = principal;
            return context;
        }
        set { }
    }
}
