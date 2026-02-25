using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Application.Auth.Commands;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Application.Tests.Handlers;

public class DeleteProfileImageHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<DeleteProfileImageHandler>> _loggerMock = new();

    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        _handler = new DeleteProfileImageHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Hnadle_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.GetRequiredUserId()).Returns(userId);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId, It.IsAny<CancellationToken>()))

            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoProfileImage()
    {
        // Arrange
        var user = new User { ProfileImageUrl = null };

        _currentUserServiceMock.Setup(s => s.GetRequiredUserId()).Returns(user.Id);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.ProfileImageUrl.Should().BeNull();

        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Users.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearProfileImage_WhenUserHasProfileImage()
    {
        // Arrange
        var user = new User { ProfileImageUrl = "profile.jpg" };

        _currentUserServiceMock.Setup(s=>s.GetRequiredUserId()).Returns(user.Id);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.ProfileImageUrl.Should().BeNull();
        
        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync("profile.jpg"), Times.Once);
        _unitOfWorkMock.Verify(u => u.Users.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
