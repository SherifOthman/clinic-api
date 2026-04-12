using ClinicManagement.Application.Behaviors;
using ClinicManagement.Domain.Common;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<Result>;
    private record TestRequestGeneric(string Value) : IRequest<Result<string>>;

    private class RequiredValueValidator : AbstractValidator<TestRequest>
    {
        public RequiredValueValidator() =>
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
    }

    private class RequiredValueGenericValidator : AbstractValidator<TestRequestGeneric>
    {
        public RequiredValueGenericValidator() =>
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([new RequiredValueValidator()]);
        var nextCalled = false;

        // Act
        await behavior.Handle(
            new TestRequest("valid value"),
            ct => { nextCalled = true; return Task.FromResult(Result.Success()); },
            default);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenValidationFails_ForResult()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([new RequiredValueValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestRequest(""), // empty value fails validation
            ct => Task.FromResult(Result.Success()),
            default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.ValidationErrors.Should().ContainKey("Value");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenValidationFails_ForResultOfT()
    {
        // This test covers the AmbiguousMatchException bug fix:
        // GetMethod("ValidationFailure") was ambiguous between Result and Result<T> overloads.
        // Arrange
        var behavior = new ValidationBehavior<TestRequestGeneric, Result<string>>([new RequiredValueGenericValidator()]);

        // Act
        var result = await behavior.Handle(
            new TestRequestGeneric(""),
            ct => Task.FromResult(Result<string>.Success("ok")),
            default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.ValidationErrors.Should().ContainKey("Value");
    }

    [Fact]
    public async Task Handle_ShouldNotCallNext_WhenValidationFails()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([new RequiredValueValidator()]);
        var nextCalled = false;

        // Act
        await behavior.Handle(
            new TestRequest(""),
            ct => { nextCalled = true; return Task.FromResult(Result.Success()); },
            default);

        // Assert
        nextCalled.Should().BeFalse();
    }
}
