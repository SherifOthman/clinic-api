using ClinicManagement.Application.Common.Behaviors;
using ClinicManagement.Application.Common.Models;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validatorMock;
    private readonly Mock<RequestHandlerDelegate<Result<string>>> _nextMock;
    private readonly ValidationBehavior<TestRequest, Result<string>> _behavior;

    public ValidationBehaviorTests()
    {
        _validatorMock = new Mock<IValidator<TestRequest>>();
        _nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();
        
        var validators = new List<IValidator<TestRequest>> { _validatorMock.Object };
        _behavior = new ValidationBehavior<TestRequest, Result<string>>(validators);
    }

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequest { Value = "test" };
        var expectedResult = Result<string>.Ok("success");
        
        var behaviorWithoutValidators = new ValidationBehavior<TestRequest, Result<string>>(new List<IValidator<TestRequest>>());
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behaviorWithoutValidators.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequest { Value = "valid" };
        var expectedResult = Result<string>.Ok("success");
        
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = new TestRequest { Value = "invalid" };
        
        var validationFailures = new List<ValidationFailure>
        {
            new("Value", "Value is required"),
            new("Value", "Value must be at least 5 characters")
        };
        
        var validationResult = new ValidationResult(validationFailures);
        
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(Result.VALIDATION_MESSAGE);
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Field == "value" && e.Message == "Value is required");
        result.Errors.Should().Contain(e => e.Field == "value" && e.Message == "Value must be at least 5 characters");
        
        _nextMock.Verify(x => x(), Times.Never);
    }

    // Test request class for testing
    public class TestRequest : IRequest<Result<string>>
    {
        public string Value { get; set; } = string.Empty;
    }
}