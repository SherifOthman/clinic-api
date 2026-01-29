using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class UserManagementServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly UserManagementService _userManagementService;

    public UserManagementServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<User>>>();
        
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, 
            options.Object, 
            passwordHasher.Object, 
            userValidators, 
            passwordValidators, 
            keyNormalizer.Object, 
            errors.Object, 
            services.Object, 
            logger.Object);
        _userManagementService = new UserManagementService(_userManagerMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User { Email = "test@example.com", UserName = "testuser" };
        var password = "Password123!";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.CreateAsync(user, password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _userManagementService.CreateUserAsync(user, password);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_WhenFailed_ShouldReturnFailureWithErrors()
    {
        // Arrange
        var user = new User { Email = "test@example.com", UserName = "testuser" };
        var password = "weak";
        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
            new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" }
        };
        var identityResult = IdentityResult.Failed(identityErrors);

        _userManagerMock.Setup(x => x.CreateAsync(user, password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _userManagementService.CreateUserAsync(user, password);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(2);
        
        var passwordError = result.Errors.First(e => e.Field == "Password");
        passwordError.Code.Should().Be("Password is too short");
        
        var emailError = result.Errors.First(e => e.Field == "Email");
        emailError.Code.Should().Be("Email already exists");
    }

    [Fact]
    public async Task CheckPasswordAsync_WhenPasswordCorrect_ShouldReturnTrue()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        var password = "Password123!";

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.CheckPasswordAsync(user, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPasswordAsync_WhenPasswordIncorrect_ShouldReturnFalse()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        var password = "WrongPassword";

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(false);

        // Act
        var result = await _userManagementService.CheckPasswordAsync(user, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new User { Email = email, UserName = "testuser" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userManagementService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var expectedUser = new User { Email = "test@example.com", UserName = username };

        _userManagerMock.Setup(x => x.FindByNameAsync(username))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userManagementService.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.UserName.Should().Be(username);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = 123;
        var expectedUser = new User { Id = userId, Email = "test@example.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userManagementService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserRolesAsync_WhenUserHasRoles_ShouldReturnRoles()
    {
        // Arrange
        var user = new User { Email = "test@example.com" };
        var expectedRoles = new List<string> { "Admin", "User" };

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _userManagementService.GetUserRolesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("Admin");
        result.Should().Contain("User");
    }

    [Theory]
    [InlineData("DuplicateEmail", "Email")]
    [InlineData("InvalidEmail", "Email")]
    [InlineData("DuplicateUserName", "UserName")]
    [InlineData("InvalidUserName", "UserName")]
    [InlineData("PasswordTooShort", "Password")]
    [InlineData("PasswordRequiresDigit", "Password")]
    [InlineData("PasswordRequiresLower", "Password")]
    [InlineData("PasswordRequiresUpper", "Password")]
    [InlineData("PasswordRequiresNonAlphanumeric", "Password")]
    [InlineData("UnknownError", "")]
    public async Task CreateUserAsync_ShouldMapErrorCodesCorrectly(string errorCode, string expectedField)
    {
        // Arrange
        var user = new User { Email = "test@example.com", UserName = "testuser" };
        var password = "password";
        var identityError = new IdentityError { Code = errorCode, Description = "Test error" };
        var identityResult = IdentityResult.Failed(identityError);

        _userManagerMock.Setup(x => x.CreateAsync(user, password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _userManagementService.CreateUserAsync(user, password);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Field.Should().Be(expectedField);
        result.Errors.First().Code.Should().Be("Test error");
    }
}