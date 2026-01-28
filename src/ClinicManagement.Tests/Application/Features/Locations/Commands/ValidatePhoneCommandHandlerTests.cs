using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Locations.Commands.ValidatePhone;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Locations.Commands;

public class ValidatePhoneCommandHandlerTests
{
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly ValidatePhoneCommandHandler _handler;

    public ValidatePhoneCommandHandlerTests()
    {
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _handler = new ValidatePhoneCommandHandler(_phoneValidationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPhoneNumber_ShouldReturnValidResult()
    {
        // Arrange
        var command = new ValidatePhoneCommand("01098021214");

        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(command.PhoneNumber, It.IsAny<string>()))
            .Returns(true);
        _phoneValidationServiceMock.Setup(x => x.FormatPhoneNumber(command.PhoneNumber, It.IsAny<string>()))
            .Returns("+201098021214");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsValid.Should().BeTrue();
        result.Value.Formatted.Should().Be("+201098021214");
    }

    [Fact]
    public async Task Handle_WithInvalidPhoneNumber_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = new ValidatePhoneCommand("123");

        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(command.PhoneNumber, It.IsAny<string>()))
            .Returns(false);
        _phoneValidationServiceMock.Setup(x => x.FormatPhoneNumber(command.PhoneNumber, It.IsAny<string>()))
            .Returns("123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenValidationServiceThrows_ShouldThrowException()
    {
        // Arrange
        var command = new ValidatePhoneCommand("01098021214");

        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(command.PhoneNumber, It.IsAny<string>()))
            .Throws(new Exception("Validation service error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Validation service error");
    }
}