using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Infrastructure.Services;
using ClinicManagement.Tests.Helpers;
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
        var service = new CodeGeneratorService(context, currentUserService, dateTimeProvider);

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
        var service = new CodeGeneratorService(context, currentUserService, dateTimeProvider);

        // Act
        var patientNumber = await service.GeneratePatientNumberAsync();

        // Assert
        patientNumber.Should().MatchRegex(@"^PAT-\d{4}-\d{6}$");
        patientNumber.Should().Contain(DateTime.UtcNow.Year.ToString());
    }
}
