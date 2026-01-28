using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using ClinicManagement.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class ChronicDiseaseRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly ChronicDiseaseRepository _repository;

    public ChronicDiseaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, _dateTimeProviderMock.Object);
        _repository = new ChronicDiseaseRepository(_context, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveDiseasesOrderedByName()
    {
        // Arrange
        var diseases = new List<ChronicDisease>
        {
            new ChronicDisease { Id = 1, Name = "Diabetes", Description = "Type 2 Diabetes", IsActive = true },
            new ChronicDisease { Id = 2, Name = "Asthma", Description = "Chronic Asthma", IsActive = true },
            new ChronicDisease { Id = 3, Name = "Hypertension", Description = "High Blood Pressure", IsActive = false },
            new ChronicDisease { Id = 4, Name = "Arthritis", Description = "Rheumatoid Arthritis", IsActive = true }
        };

        await _context.ChronicDiseases.AddRangeAsync(diseases);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        var activeList = result.ToList();
        activeList.Should().HaveCount(3);
        activeList.Should().OnlyContain(d => d.IsActive);
        
        // Verify ordering by name
        activeList[0].Name.Should().Be("Arthritis");
        activeList[1].Name.Should().Be("Asthma");
        activeList[2].Name.Should().Be("Diabetes");
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoActiveDiseases_ShouldReturnEmptyList()
    {
        // Arrange
        var diseases = new List<ChronicDisease>
        {
            new ChronicDisease { Id = 1, Name = "Diabetes", IsActive = false }
        };

        await _context.ChronicDiseases.AddRangeAsync(diseases);
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