using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Queries.GetMe;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Queries;

public class GetMeQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly GetMeQueryHandler _handler;

    public GetMeQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _handler = new GetMeQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object, 
            _userManagementServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnUserDto()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagementServiceMock.Setup(x => x.GetUserRolesAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "ClinicOwner" });

        var query = new GetMeQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        var userId = 1;
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetMeQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Message.Should().Be(ApplicationErrors.Authentication.USER_NOT_FOUND);
    }

    private static User CreateTestUser(int id)
    {
        return new User
        {
            Id = id,
            UserName = $"john@example.com",
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
    }
}