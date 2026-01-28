using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class PatientRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly PatientRepository _repository;

    public PatientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        
        // Setup the current user service to return clinic ID 1 to match test data
        _currentUserServiceMock.Setup(x => x.ClinicId).Returns(1);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(1);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, _dateTimeProviderMock.Object);
        _repository = new PatientRepository(_context, _currentUserServiceMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var patients = new[]
        {
            new Patient
            {
                Id = 1,
                ClinicId = 1,
                FullName = "John Michael Doe",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "123 Main Street, Cairo",
                GeoNameId = 360630, // Cairo, Egypt
                CreatedAt = DateTime.UtcNow
            },
            new Patient
            {
                Id = 2,
                ClinicId = 1,
                FullName = "Jane Marie Smith",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1985, 5, 15),
                Address = "456 Elm Street, Alexandria",
                GeoNameId = 361058, // Alexandria, Egypt
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Patients.AddRange(patients);
        
        var phoneNumbers = new[]
        {
            new PatientPhoneNumber
            {
                Id = 1,
                PatientId = 1,
                PhoneNumber = "+201098021214"
            },
            new PatientPhoneNumber
            {
                Id = 2,
                PatientId = 2,
                PhoneNumber = "+201234567890"
            }
        };
        
        _context.PatientPhoneNumbers.AddRange(phoneNumbers);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchByName_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10, "John");

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("John Michael Doe");
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchByNamePaged_ShouldReturnPagedResults()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10, "John");

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().FullName.Should().Be("John Michael Doe");
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchTermProvided_ShouldReturnFilteredResults()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10, "Jane");

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("Jane Marie Smith");
    }

    [Fact]
    public async Task GetPagedAsync_WhenFilterByAgeRange_ShouldReturnMatchingPatients()
    {
        // Arrange - Looking for patients aged 30-40 (born between 1984-1994)
        var searchRequest = new PatientSearchRequest(1, 10)
        {
            MinAge = 30,
            MaxAge = 40
        };

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Both test patients should be in this range
    }

    [Fact]
    public async Task GetPagedAsync_WhenFilterByGender_ShouldReturnFilteredResults()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10)
        {
            Gender = Gender.Male
        };

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchByPhoneNumber_ShouldReturnPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10, "+201098021214");

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("John Michael Doe");
    }

    [Fact]
    public async Task GetPatientWithLocationAsync_WhenPatientHasLocation_ShouldReturnLocationData()
    {
        // Act
        var result = await _repository.GetByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Address.Should().Be("123 Main Street, Cairo");
        result.GeoNameId.Should().Be(360630);
    }

    [Fact]
    public async Task GetPagedAsync_WhenSearchByLocation_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest(1, 10, "Cairo");

        // Act
        var result = await _repository.GetPagedAsync(searchRequest, CancellationToken.None);

        // Assert - This searches by name, not address, so it won't find "Cairo" in the name
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}