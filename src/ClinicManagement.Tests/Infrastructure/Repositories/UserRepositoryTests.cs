using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly UserRepository _repository;

    private static User CreateTestUser(int id, string email, string firstName, string lastName, bool emailConfirmed = false)
    {
        return new User
        {
            Id = id,
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = emailConfirmed,
            CreatedAt = DateTime.UtcNow
        };
    }

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        
        _context = new ApplicationDbContext(options, _currentUserServiceMock.Object, dateTimeProviderMock.Object);
        
        var store = new Mock<IUserStore<User>>();
        var identityOptions = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<User>>>();
        
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, 
            identityOptions.Object, 
            passwordHasher.Object, 
            userValidators, 
            passwordValidators, 
            keyNormalizer.Object, 
            errors.Object, 
            services.Object, 
            logger.Object);
        
        _repository = new UserRepository(_context, _currentUserServiceMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateTestUser(1, email, "John", "Doe");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateTestUser(1, email, "John", "Doe");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersByRoleAsync_WhenUsersExist_ShouldReturnUsers()
    {
        // Arrange
        var role = "Admin";
        var users = new List<User>
        {
            CreateTestUser(1, "admin1@example.com", "Admin", "One"),
            CreateTestUser(2, "admin2@example.com", "Admin", "Two")
        };

        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(role))
            .ReturnsAsync(users);

        // Act
        var result = await _repository.GetUsersByRoleAsync(role);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(user => user.FirstName.Contains("Admin"));
    }

    [Fact]
    public async Task GetPagedAsync_WhenNoFilters_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "user1@example.com", "John", "Doe"),
            CreateTestUser(2, "user2@example.com", "Jane", "Smith"),
            CreateTestUser(3, "user3@example.com", "Bob", "Johnson")
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTerm_ShouldReturnFilteredUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "john@example.com", "John", "Doe"),
            CreateTestUser(2, "jane@example.com", "Jane", "Smith"),
            CreateTestUser(3, "bob@example.com", "Bob", "Johnson")
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new UserSearchRequest(1, 10, "John");

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(2); // John Doe and Bob Johnson
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(user => 
            user.FirstName.Contains("John") || 
            user.LastName.Contains("John") || 
            user.Email!.Contains("john"));
    }

    [Fact]
    public async Task GetPagedAsync_WithEmailConfirmedFilter_ShouldReturnFilteredUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "confirmed@example.com", "John", "Doe", emailConfirmed: true),
            CreateTestUser(2, "unconfirmed@example.com", "Jane", "Smith", emailConfirmed: false)
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new UserSearchRequest(1, 10) { EmailConfirmed = true };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_WithSorting_ShouldReturnSortedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "c@example.com", "Charlie", "Brown"),
            CreateTestUser(2, "a@example.com", "Alice", "Johnson"),
            CreateTestUser(3, "b@example.com", "Bob", "Smith")
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new UserSearchRequest(1, 10) { SortBy = "firstname", SortDescending = false };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.First().FirstName.Should().Be("Alice");
        result.Items.Last().FirstName.Should().Be("Charlie");
    }

    [Fact]
    public async Task GetPagedAsync_WithDescendingSorting_ShouldReturnSortedUsersDescending()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "a@example.com", "Alice", "Johnson"),
            CreateTestUser(2, "b@example.com", "Bob", "Smith"),
            CreateTestUser(3, "c@example.com", "Charlie", "Brown")
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new UserSearchRequest(1, 10) { SortBy = "firstname", SortDescending = true };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.First().FirstName.Should().Be("Charlie");
        result.Items.Last().FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 1; i <= 15; i++)
        {
            users.Add(CreateTestUser(i, $"user{i}@example.com", $"User{i}", "Test"));
        }

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest { PageNumber = 2, PageSize = 5 };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(3);
        result.Items.First().FirstName.Should().Be("User6"); // Should start from 6th user
    }

    [Theory]
    [InlineData("firstname", false, "Alice")]
    [InlineData("firstname", true, "Charlie")]
    [InlineData("lastname", false, "Brown")]
    [InlineData("lastname", true, "Smith")]
    [InlineData("email", false, "a@example.com")]
    [InlineData("email", true, "c@example.com")]
    [InlineData("invalid", false, "Charlie")] // Should default to ID sorting (ID=1 first)
    public async Task GetPagedAsync_WithDifferentSortOptions_ShouldSortCorrectly(string sortBy, bool sortDescending, string expectedFirstValue)
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "c@example.com", "Charlie", "Brown"),
            CreateTestUser(2, "a@example.com", "Alice", "Johnson"),
            CreateTestUser(3, "b@example.com", "Bob", "Smith")
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var request = new UserSearchRequest(1, 10) { SortBy = sortBy, SortDescending = sortDescending };

        // Act
        var result = await _repository.GetPagedAsync(request);

        // Assert
        var firstUser = result.Items.First();
        var actualValue = sortBy.ToLower() switch
        {
            "firstname" => firstUser.FirstName,
            "lastname" => firstUser.LastName,
            "email" => firstUser.Email,
            _ => firstUser.FirstName // Default case
        };
        
        actualValue.Should().Be(expectedFirstValue);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}