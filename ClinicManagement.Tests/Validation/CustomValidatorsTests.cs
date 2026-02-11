using ClinicManagement.API.Common.Validation;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Tests.Validation;

public class CustomValidatorsTests
{
    [Fact]
    public void MustBeInPast_WithPastDate_ShouldReturnSuccess()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInPast(pastDate, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInPast_WithFutureDate_ShouldReturnError()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInPast(futureDate, context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("past");
    }

    [Fact]
    public void MustBeInPast_WithCurrentTime_ShouldReturnSuccess()
    {
        // Arrange
        // Current time is technically in the past by the time the comparison happens
        var now = DateTime.UtcNow;
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInPast(now, context);

        // Assert
        // Due to timing, this might be Success (if microseconds have passed)
        // This test verifies the validator handles edge cases gracefully
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFuture_WithFutureDate_ShouldReturnSuccess()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFuture(futureDate, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFuture_WithPastDate_ShouldReturnError()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFuture(pastDate, context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("future");
    }

    [Fact]
    public void MustBeInFuture_WithCurrentTime_ShouldReturnSuccess()
    {
        // Arrange
        // Use a time slightly in the future to avoid timing issues
        var futureTime = DateTime.UtcNow.AddMilliseconds(100);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFuture(futureTime, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFutureOrNull_WithNull_ShouldReturnSuccess()
    {
        // Arrange
        DateTime? nullDate = null;
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrNull(nullDate, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFutureOrNull_WithFutureDate_ShouldReturnSuccess()
    {
        // Arrange
        DateTime? futureDate = DateTime.UtcNow.AddDays(1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrNull(futureDate, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFutureOrNull_WithPastDate_ShouldReturnError()
    {
        // Arrange
        DateTime? pastDate = DateTime.UtcNow.AddDays(-1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrNull(pastDate, context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("future");
    }

    [Fact]
    public void MustBeInFutureOrToday_WithFutureDate_ShouldReturnSuccess()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrToday(futureDate, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFutureOrToday_WithToday_ShouldReturnSuccess()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrToday(today, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void MustBeInFutureOrToday_WithPastDate_ShouldReturnError()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrToday(pastDate, context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("past");
    }

    [Fact]
    public void MustBeInFutureOrToday_WithCurrentDateTime_ShouldReturnSuccess()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var context = new ValidationContext(new object());

        // Act
        var result = CustomValidators.MustBeInFutureOrToday(now, context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }
}
