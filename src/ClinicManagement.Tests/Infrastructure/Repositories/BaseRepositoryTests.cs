using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class BaseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly BaseRepository<Patient> _repository;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        
        _currentUserServiceMock.Setup(x => x.ClinicId).Returns(1);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(1);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, _dateTimeProviderMock.Object);
        _repository = new BaseRepository<Patient>(_context);
    }

    [Fact]
    public async Task GetPagedAsync_WithData_ShouldReturnCorrectPagination()
    {
        // Arrange
        var patients = new List<Patient>
        {
            new Patient { ClinicId = 1, FullName = "John Doe" },
            new Patient { ClinicId = 1, FullName = "Jane Smith" },
            new Patient { ClinicId = 1, FullName = "Bob Johnson" },
            new Patient { ClinicId = 1, FullName = "Alice Brown" },
            new Patient { ClinicId = 1, FullName = "Charlie Wilson" }
        };

        await _context.Patients.AddRangeAsync(patients);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest(1, 3);

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithEmptyDatabase_ShouldReturnEmptyResult()
    {
        // Arrange
        var request = new PaginationRequest(1, 10);

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}