using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using ClinicManagement.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class SubscriptionPlanRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly SubscriptionPlanRepository _repository;

    public SubscriptionPlanRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, _dateTimeProviderMock.Object);
        _repository = new SubscriptionPlanRepository(_context);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActivePlansOrderedByPrice()
    {
        // Arrange
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan { Id = 1, Name = "Premium", Price = 59.99m, IsActive = true },
            new SubscriptionPlan { Id = 2, Name = "Basic", Price = 29.99m, IsActive = true },
            new SubscriptionPlan { Id = 3, Name = "Enterprise", Price = 99.99m, IsActive = false },
            new SubscriptionPlan { Id = 4, Name = "Starter", Price = 19.99m, IsActive = true }
        };

        await _context.SubscriptionPlans.AddRangeAsync(plans);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        var activeList = result.ToList();
        activeList.Should().HaveCount(3);
        activeList.Should().OnlyContain(p => p.IsActive);
        
        // Verify ordering by price
        activeList[0].Price.Should().Be(19.99m);
        activeList[1].Price.Should().Be(29.99m);
        activeList[2].Price.Should().Be(59.99m);
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoActivePlans_ShouldReturnEmptyList()
    {
        // Arrange
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan { Id = 1, Name = "Premium", Price = 59.99m, IsActive = false }
        };

        await _context.SubscriptionPlans.AddRangeAsync(plans);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}