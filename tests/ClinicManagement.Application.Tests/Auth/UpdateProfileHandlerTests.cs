using ClinicManagement.Application.Abstractions.Data;
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
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly UpdateProfileHandler _handler;

    public UpdateProfileHandlerTests()
    {
        _handler = new UpdateProfileHandler(_uow, _currentUserMock.Object, NullLogger<UpdateProfileHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());

        var result = await _handler.Handle(new UpdateProfileCommand("A", "B", "ab", null, "Male"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFields_WhenUserExists()
    {
        var user = TestHandlerHelpers.CreateTestUser("user@test.com");
        user.UserName = "olduser";
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);

        var result = await _handler.Handle(
            new UpdateProfileCommand("New", "Name", "newuser", "+966500000001", "Male"), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Users.GetByIdWithPersonAsync(user.Id);
        updated!.Person.FullName.Should().Be("New");
        updated.UserName.Should().Be("newuser");
        updated.Person.Gender.Should().Be(Gender.Male);
        updated.PhoneNumber.Should().Be("+966500000001");
    }

    [Fact]
    public async Task Handle_ShouldSetPhoneToNull_WhenPhoneIsWhitespace()
    {
        var user = TestHandlerHelpers.CreateTestUser("test@test.com");
        user.UserName = "testuser";
        user.PhoneNumber = "+966500000000";
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);

        await _handler.Handle(new UpdateProfileCommand("Test", "User", "testuser", "   ", "Male"), default);

        var updated = await _uow.Users.GetByIdAsync(user.Id);
        updated!.PhoneNumber.Should().BeNull();
    }
}
