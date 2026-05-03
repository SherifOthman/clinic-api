using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Application.Features.Auth.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class DeleteProfileImageHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new DeleteProfileImageHandler(
            _uowMock.Object, _currentUserMock.Object, _fileStorageMock.Object,
            NullLogger<DeleteProfileImageHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(userId);
        _usersMock.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoImage()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearUrl_WhenUserHasImage()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        user.ProfileImageUrl = "profile.jpg";
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(x => x.DeleteFileAsync("profile.jpg", default), Times.Once);
        user.ProfileImageUrl.Should().BeNull();
    }
}
