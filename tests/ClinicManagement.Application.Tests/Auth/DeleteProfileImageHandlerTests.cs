using ClinicManagement.Application.Abstractions.Data;
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
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        _handler = new DeleteProfileImageHandler(
            _uow, _currentUserMock.Object, _fileStorageMock.Object,
            NullLogger<DeleteProfileImageHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoImage()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearUrl_WhenUserHasImage()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        user.Person.ProfileImageUrl = "profile.jpg";
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);

        var result = await _handler.Handle(new DeleteProfileImageCommand(), default);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(x => x.DeleteFileAsync("profile.jpg", default), Times.Once);

        var updated = await _uow.Users.GetByIdWithPersonAsync(user.Id);
        updated!.Person.ProfileImageUrl.Should().BeNull();
    }
}
