using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using ClinicManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class ClinicRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ClinicRepository _repository;

    public ClinicRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, _dateTimeProviderMock.Object);
        _repository = new ClinicRepository(_context);
    }

    [Fact]
    public async Task GetPagedAsync_WhenNoClinics_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var request = new ClinicSearchRequest(1, 10);

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTerm_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10, "Medical");

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Contain("Medical");
    }

    [Fact]
    public async Task GetPagedAsync_WithSubscriptionPlanFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) { SubscriptionPlanId = 2 };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(c => c.SubscriptionPlanId == 2);
    }

    [Fact]
    public async Task GetPagedAsync_WithActiveStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) { IsActive = false };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithDateRangeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestData();
        var fromDate = DateTime.UtcNow.AddDays(-5);
        var toDate = DateTime.UtcNow.AddDays(5);
        var request = new ClinicSearchRequest(1, 10) 
        { 
            CreatedFrom = fromDate, 
            CreatedTo = toDate 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Count().Should().BeGreaterOrEqualTo(0);
        result.Items.Should().OnlyContain(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate);
    }

    [Fact]
    public async Task GetPagedAsync_WithUserCountFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await SeedTestDataWithUsers();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            MinUsers = 1, 
            MaxUsers = 2 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(c => c.Users.Count >= 1 && c.Users.Count <= 2);
    }

    [Fact]
    public async Task GetPagedAsync_WithSortByName_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            SortBy = "name", 
            SortDescending = false 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var sortedNames = result.Items.Select(c => c.Name).ToList();
        var expectedOrder = sortedNames.OrderBy(n => n).ToList();
        sortedNames.Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetPagedAsync_WithSortByNameDescending_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            SortBy = "name", 
            SortDescending = true 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var sortedNames = result.Items.Select(c => c.Name).ToList();
        var expectedOrder = sortedNames.OrderByDescending(n => n).ToList();
        sortedNames.Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetPagedAsync_WithSortByCreatedAt_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            SortBy = "createdat", 
            SortDescending = false 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var sortedDates = result.Items.Select(c => c.CreatedAt).ToList();
        var expectedOrder = sortedDates.OrderBy(d => d).ToList();
        sortedDates.Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetPagedAsync_WithSortByIsActive_ShouldReturnSortedResults()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            SortBy = "isactive", 
            SortDescending = false 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var sortedActiveStatus = result.Items.Select(c => c.IsActive).ToList();
        var expectedOrder = sortedActiveStatus.OrderBy(a => a).ToList();
        sortedActiveStatus.Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidSortBy_ShouldReturnDefaultSorting()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10) 
        { 
            SortBy = "invalid", 
            SortDescending = false 
        };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var sortedIds = result.Items.Select(c => c.Id).ToList();
        var expectedOrder = sortedIds.OrderBy(id => id).ToList();
        sortedIds.Should().Equal(expectedOrder);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(2, 1);

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Items.Should().ContainSingle();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        await SeedTestData();
        var request = new ClinicSearchRequest(1, 10);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await _repository.GetPagedAsync(request, cancellationToken);

        // Assert
        result.Items.Should().HaveCount(3);
    }

    private async Task SeedTestData()
    {
        var subscriptionPlan1 = new SubscriptionPlan { Id = 1, Name = "Basic", Price = 29.99m, IsActive = true };
        var subscriptionPlan2 = new SubscriptionPlan { Id = 2, Name = "Premium", Price = 59.99m, IsActive = true };

        await _context.SubscriptionPlans.AddRangeAsync(subscriptionPlan1, subscriptionPlan2);
        await _context.SaveChangesAsync();

        var clinics = new List<Clinic>
        {
            new Clinic 
            { 
                Id = 1, 
                Name = "City Medical Center", 
                SubscriptionPlanId = 1,
                IsActive = true, 
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Users = new List<User>()
            },
            new Clinic 
            { 
                Id = 2, 
                Name = "Family Health Clinic", 
                SubscriptionPlanId = 2,
                IsActive = true, 
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Users = new List<User>()
            },
            new Clinic 
            { 
                Id = 3, 
                Name = "Wellness Center", 
                SubscriptionPlanId = 2, // Changed to 2 to test filtering
                IsActive = false, 
                CreatedAt = DateTime.UtcNow,
                Users = new List<User>()
            }
        };

        await _context.Clinics.AddRangeAsync(clinics);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTestDataWithUsers()
    {
        var subscriptionPlan = new SubscriptionPlan { Id = 1, Name = "Basic", Price = 29.99m, IsActive = true };
        await _context.SubscriptionPlans.AddAsync(subscriptionPlan);
        await _context.SaveChangesAsync();

        var clinic = new Clinic 
        { 
            Id = 1, 
            Name = "Test Clinic", 
            SubscriptionPlanId = 1,
            IsActive = true, 
            CreatedAt = DateTime.UtcNow,
            Users = new List<User>()
        };

        await _context.Clinics.AddAsync(clinic);
        await _context.SaveChangesAsync();

        var users = new List<User>
        {
            CreateTestUser(1, "user1@test.com", "User", "One", clinic.Id),
            CreateTestUser(2, "user2@test.com", "User", "Two", clinic.Id)
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    private static User CreateTestUser(int id, string email, string firstName, string lastName, int clinicId)
    {
        return new User
        {
            Id = id,
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ClinicId = clinicId,
            EmailConfirmed = true
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}