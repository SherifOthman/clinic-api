using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Auth.Commands.ChangePassword;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers
{
    public class ChangePasswordHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();
        private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock = new();

        private readonly ChangePasswordHandler _handler;

        public ChangePasswordHandlerTests()
        {
            _handler = new ChangePasswordHandler(
                _unitOfWorkMock.Object,
                _passwordHasherMock.Object,
                _currentUserMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task Handle_ShouldFail_WhenUserDoesNotExist()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            _currentUserMock
                .Setup(x => x.GetRequiredClinicId())
                .Returns(userId);

            _unitOfWorkMock.
                Setup(x => x.Users.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new ChangePasswordCommand("old", "new");

            // Act
            var result = await _handler.Handle(command, default);


            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var user = new User { PasswordHash = "hash" };

            _currentUserMock
                .Setup(x => x.GetRequiredClinicId())
                .Returns(user.Id);

            _unitOfWorkMock
                .Setup(x => x.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.VerifyPassword("worng", "hash"))
                .Returns(false);

            var command = new ChangePasswordCommand("worng", "new");

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenCurrentPasswordIsCorrect()
        {
            // Arrange
            var user = new User { PasswordHash = "oldhash" };

            _currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Returns(user.Id);

            _unitOfWorkMock
                .Setup(x=>x.Users.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x=>x.VerifyPassword("old", "oldhash"))
                .Returns(true);

            _passwordHasherMock
                .Setup(x=>x.HashPassword("new"))
                .Returns("newhash");

            var command = new ChangePasswordCommand("old", "new");

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.PasswordHash.Should().Be("newhash");

            _unitOfWorkMock.Verify(x=>x.Users.UpdateAsync(user, It.IsAny<CancellationToken>())
            ,Times.Once);


        }
    }

}