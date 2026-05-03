using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.QueryModels;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<ITokenIssuer> _tokenIssuerMock = new();
    private readonly Mock<IAuditWriter> _auditWriterMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);

        // Default happy-path token response
        _tokenIssuerMock
            .Setup(x => x.ResolveContextAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<string>>(), default))
            .ReturnsAsync(Result.Success(TokenContext.Empty));

        _tokenIssuerMock
            .Setup(x => x.IssueTokenPairAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<TokenContext>(), default))
            .ReturnsAsync(new TokenResponseDto("access-token", "refresh-token"));

        _handler = new LoginHandler(
            _uowMock.Object, _userManagerMock.Object, _tokenIssuerMock.Object,
            _auditWriterMock.Object, NullLogger<LoginHandler>.Instance);
    }

    private void SetupUser(User user, List<string> roles)
    {
        _usersMock
            .Setup(x => x.GetByEmailOrUsernameWithRolesAsync(user.Email!, default))
            .ReturnsAsync(new UserWithRoles(user, roles));
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _usersMock.Setup(x => x.GetByEmailOrUsernameWithRolesAsync("nobody@test.com", default))
            .ReturnsAsync((UserWithRoles?)null);

        var result = await _handler.Handle(new LoginCommand("nobody@test.com", "pass", false), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPasswordIsWrong()
    {
        var user = TestHandlerHelpers.CreateTestUser("user@test.com");
        SetupUser(user, ["ClinicOwner"]);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(new LoginCommand("user@test.com", "wrong", false), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAccountIsLocked()
    {
        var user = TestHandlerHelpers.CreateTestUser("locked@test.com");
        SetupUser(user, ["ClinicOwner"]);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetLockoutEndDateAsync(user))
            .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(25));

        var result = await _handler.Handle(new LoginCommand("locked@test.com", "pass", false), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ACCOUNT_LOCKED");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndReturnTokens_WhenCredentialsAreValid()
    {
        var user = TestHandlerHelpers.CreateTestUser("valid@test.com");
        SetupUser(user, ["ClinicOwner"]);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "correct")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(new LoginCommand("valid@test.com", "correct", false), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_ShouldStampLastLoginAt_OnSuccess()
    {
        var user = TestHandlerHelpers.CreateTestUser("stamp@test.com");
        SetupUser(user, ["ClinicOwner"]);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "pass")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _handler.Handle(new LoginCommand("stamp@test.com", "pass", false), default);

        user.LastLoginAt.Should().NotBeNull();
    }
}
