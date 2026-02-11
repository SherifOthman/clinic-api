using ClinicManagement.API.Common.Extensions;
using FluentAssertions;

namespace ClinicManagement.Tests.Extensions;

public class QueryExtensionsTests
{
    [Fact]
    public void WhereIf_WithTrueCondition_ShouldApplyPredicate()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = items.WhereIf(true, x => x > 3).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { 4, 5 });
    }

    [Fact]
    public void WhereIf_WithFalseCondition_ShouldNotApplyPredicate()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

        // Act
        var result = items.WhereIf(false, x => x > 3).ToList();

        // Assert
        result.Should().HaveCount(5);
        result.Should().Contain(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Paginate_ShouldReturnCorrectPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).AsQueryable();

        // Act
        var page1 = items.Paginate(1, 10).ToList();
        var page2 = items.Paginate(2, 10).ToList();
        var page3 = items.Paginate(3, 10).ToList();

        // Assert
        page1.Should().HaveCount(10);
        page1.First().Should().Be(1);
        page1.Last().Should().Be(10);

        page2.Should().HaveCount(10);
        page2.First().Should().Be(11);
        page2.Last().Should().Be(20);

        page3.Should().HaveCount(10);
        page3.First().Should().Be(21);
        page3.Last().Should().Be(30);
    }

    [Fact]
    public void Paginate_WithLastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).AsQueryable();

        // Act
        var lastPage = items.Paginate(3, 10).ToList();

        // Assert
        lastPage.Should().HaveCount(5);
        lastPage.First().Should().Be(21);
        lastPage.Last().Should().Be(25);
    }

    [Fact]
    public void Paginate_WithPageBeyondData_ShouldReturnEmpty()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).AsQueryable();

        // Act
        var result = items.Paginate(5, 10).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchBy_WithMatchingTerm_ShouldReturnMatches()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com" },
            new TestItem { Name = "Jane Smith", Email = "jane@example.com" },
            new TestItem { Name = "Bob Johnson", Email = "bob@example.com" }
        }.AsQueryable();

        // Act - Search is case-sensitive, searching for "john"
        var result = items.SearchBy("john", x => x.Name, x => x.Email).ToList();

        // Assert - Only matches where "john" appears in lowercase (email only)
        result.Should().HaveCount(1);
        result.Should().Contain(x => x.Email == "john@example.com");
    }

    [Fact]
    public void SearchBy_WithNullSearchTerm_ShouldReturnAll()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com" },
            new TestItem { Name = "Jane Smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act
        var result = items.SearchBy(null, x => x.Name).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void SearchBy_WithEmptySearchTerm_ShouldReturnAll()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com" },
            new TestItem { Name = "Jane Smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act
        var result = items.SearchBy("", x => x.Name).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void SearchBy_WithNoMatches_ShouldReturnEmpty()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com" },
            new TestItem { Name = "Jane Smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act
        var result = items.SearchBy("xyz", x => x.Name, x => x.Email).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchBy_IsCaseSensitive()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com" },
            new TestItem { Name = "jane smith", Email = "jane@example.com" }
        }.AsQueryable();

        // Act - searching for lowercase "john"
        var result = items.SearchBy("john", x => x.Name, x => x.Email).ToList();

        // Assert - Only matches email, not "John" in name (case-sensitive)
        result.Should().HaveCount(1);
        result.First().Email.Should().Be("john@example.com");
    }

    [Fact]
    public void ChainedQueries_ShouldWorkTogether()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "John Doe", Email = "john@example.com", Age = 25 },
            new TestItem { Name = "Jane Smith", Email = "jane@example.com", Age = 30 },
            new TestItem { Name = "Bob Johnson", Email = "bob@example.com", Age = 35 },
            new TestItem { Name = "Alice Brown", Email = "alice@example.com", Age = 40 }
        }.AsQueryable();

        // Act - Chain multiple extensions (search is case-sensitive)
        var result = items
            .WhereIf(true, x => x.Age >= 30)
            .SearchBy("bob", x => x.Name, x => x.Email) // lowercase "bob" matches email
            .Paginate(1, 10)
            .ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Bob Johnson");
    }

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
