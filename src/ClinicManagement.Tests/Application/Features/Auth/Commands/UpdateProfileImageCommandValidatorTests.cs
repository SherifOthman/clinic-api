using ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class UpdateProfileImageCommandValidatorTests
{
    private readonly UpdateProfileImageCommandValidator _validator;

    public UpdateProfileImageCommandValidatorTests()
    {
        _validator = new UpdateProfileImageCommandValidator();
    }

    [Fact]
    public void Validate_WhenImageIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProfileImageCommand { Image = null! };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Image" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WhenImageIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.Length).Returns(0);
        mockFile.Setup(x => x.ContentType).Returns("image/png");
        mockFile.Setup(x => x.FileName).Returns("");
        var command = new UpdateProfileImageCommand { Image = mockFile.Object };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Image.FileName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void Validate_WhenImageIsTooLarge_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.Length).Returns(6 * 1024 * 1024); // 6MB
        mockFile.Setup(x => x.ContentType).Returns("image/png");
        mockFile.Setup(x => x.FileName).Returns("test.png");
        var command = new UpdateProfileImageCommand { Image = mockFile.Object };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Image.Length" && e.ErrorMessage.Contains("5MB"));
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/pdf")]
    [InlineData("video/mp4")]
    public void Validate_WhenContentTypeIsNotImage_ShouldHaveValidationError(string contentType)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.Length).Returns(1024);
        mockFile.Setup(x => x.ContentType).Returns(contentType);
        mockFile.Setup(x => x.FileName).Returns("test.file");
        var command = new UpdateProfileImageCommand { Image = mockFile.Object };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Image.ContentType" && e.ErrorMessage.Contains("image"));
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public void Validate_WhenValidImageFile_ShouldPassValidation(string contentType)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.Length).Returns(1024);
        mockFile.Setup(x => x.ContentType).Returns(contentType);
        mockFile.Setup(x => x.FileName).Returns("test.png");
        var command = new UpdateProfileImageCommand { Image = mockFile.Object };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}