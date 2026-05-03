using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class UpdateProfileHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly UpdateProfileHandler _handler;

    public UpdateProfileHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new UpdateProfileHandler(_uowMock.Object, _currentUserMock.Object, NullLogger<UpdateProfileHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(userId);
        _usersMock.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new UpdateProfileCommand("A", "ab", null, "Male"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenUserExists()
    {
        var user = TestHandlerHelpers.CreateTestUser("user@test.com");
        user.UserName = "olduser";
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _handler.Handle(
            new UpdateProfileCommand("New Name", "newuser", "+966500000001", "Male"), default);

        result.IsSuccess.Should().BeTrue();
        user.FullName.Should().Be("New Name");
        user.UserName.Should().Be("newuser");
        user.Gender.Should().Be(Gender.Male);
        user.PhoneNumber.Should().Be("+966500000001");
    }

    [Fact]
    public async Task Handle_ShouldSetPhoneToNull_WhenPhoneIsWhitespace()
    {
        var user = TestHandlerHelpers.CreateTestUser("test@test.com");
        user.UserName = "testuser";
        user.PhoneNumber = "+966500000000";
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        await _handler.Handle(new UpdateProfileCommand("Test User", "testuser", "   ", "Male"), default);

        user.PhoneNumber.Should().BeNull();
    }
}
