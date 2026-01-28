using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Infrastructure.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Extensions;

public class QueryablePaginationExtensionsTests : IDisposable
{
    private readonly TestDbContext _context;

    public QueryablePaginationExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var testEntities = Enumerable.Range(1, 25)
            .Select(i => new TestEntity { Id = i, Name = $"Entity {i}" })
            .ToList();

        _context.TestEntities.AddRange(testEntities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ToPaginatedResultAsync_WithValidRequest_ShouldReturnCorrectPage()
    {
        // Arrange
        var request = new PaginationRequest(1, 10);
        var query = _context.TestEntities.AsQueryable();

        // Act
        var result = await query.ToPaginatedResultAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ToPaginatedResultAsync_WithEmptyQuery_ShouldReturnEmptyResult()
    {
        // Arrange
        var request = new PaginationRequest(1, 10);
        var query = _context.TestEntities.Where(e => e.Id > 100); // No matches

        // Act
        var result = await query.ToPaginatedResultAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // Test entities and context
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
        }
    }
}