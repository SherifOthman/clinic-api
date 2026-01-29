using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _service;
    private readonly string _testUploadPath;
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;
    private readonly Mock<IFileSystem> _fileSystemMock;

    public LocalFileStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();
        _fileSystemMock = new Mock<IFileSystem>();
        _testUploadPath = Path.Combine(Path.GetTempPath(), "test-uploads", Guid.NewGuid().ToString());
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:UploadPath"] = _testUploadPath,
                ["FileStorage:BaseUrl"] = "/test-uploads",
                ["FileStorage:MaxFileSizeBytes"] = "1048576" // 1MB
            })
            .Build();

        // Setup file system mocks
        _fileSystemMock.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));
        _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        _fileSystemMock.Setup(x => x.Combine(It.IsAny<string[]>()))
            .Returns<string[]>(paths => Path.Combine(paths));
        _fileSystemMock.Setup(x => x.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>()))
            .Returns(() => new MemoryStream());
        _fileSystemMock.Setup(x => x.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>()))
            .Returns(() => new MemoryStream());
        _fileSystemMock.Setup(x => x.DeleteFile(It.IsAny<string>()));
        _fileSystemMock.Setup(x => x.GetExtension(It.IsAny<string>()))
            .Returns<string>(path => Path.GetExtension(path));
        _fileSystemMock.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new LocalFileStorageService(configuration, _loggerMock.Object, _fileSystemMock.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WhenValidImageFile_ShouldUploadSuccessfully()
    {
        // Arrange
        var content = "fake image content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileName = "test.png";
        var contentType = "image/png";

        // Act
        var result = await _service.UploadFileAsync(stream, fileName, contentType, "test-folder");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FilePath.Should().Contain("test-folder");
        result.Value.FileName.Should().EndWith(".png");
        result.Value.ContentType.Should().Be(contentType);
        result.Value.FileSize.Should().Be(stream.Length);
    }

    [Fact]
    public async Task UploadFileAsync_WhenFileTooLarge_ShouldReturnFailure()
    {
        // Arrange
        var largeContent = new byte[2 * 1024 * 1024]; // 2MB
        var stream = new MemoryStream(largeContent);
        var fileName = "large.png";
        var contentType = "image/png";

        // Act
        var result = await _service.UploadFileAsync(stream, fileName, contentType);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("FILE.TOO_LARGE");
    }

    [Fact]
    public async Task UploadFileAsync_WhenInvalidFileType_ShouldReturnFailure()
    {
        // Arrange
        var content = "fake content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileName = "test.txt";
        var contentType = "text/plain";

        // Act
        var result = await _service.UploadFileAsync(stream, fileName, contentType);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("FILE.INVALID_TYPE");
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var content = "fake image content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileName = "test.png";
        var contentType = "image/png";

        // Upload file first
        var uploadResult = await _service.UploadFileAsync(stream, fileName, contentType);
        uploadResult.Success.Should().BeTrue();

        // Act
        var deleteResult = await _service.DeleteFileAsync(uploadResult.Value!.FilePath);

        // Assert
        deleteResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange
        var nonExistentPath = "non-existent/file.png";

        // Act
        var result = await _service.DeleteFileAsync(nonExistentPath);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetFileAsync_WhenFileExists_ShouldReturnStream()
    {
        // Arrange
        var content = "fake image content";
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        var fileName = "test.png";
        var contentType = "image/png";

        // Setup file system to return the content when reading
        var memoryStream = new MemoryStream(contentBytes);
        _fileSystemMock.Setup(x => x.CreateFileStream(It.IsAny<string>(), FileMode.Open, FileAccess.Read))
            .Returns(memoryStream);

        // Upload file first
        var uploadResult = await _service.UploadFileAsync(stream, fileName, contentType);
        uploadResult.Success.Should().BeTrue();

        // Act
        var getResult = await _service.GetFileAsync(uploadResult.Value!.FilePath);

        // Assert
        getResult.Success.Should().BeTrue();
        getResult.Value.Should().NotBeNull();
        
        using var reader = new StreamReader(getResult.Value!);
        var retrievedContent = await reader.ReadToEndAsync();
        retrievedContent.Should().Be(content);
    }

    [Fact]
    public async Task GetFileAsync_WhenFileDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentPath = "non-existent/file.png";
        
        // Setup file system to return false for non-existent file
        _fileSystemMock.Setup(x => x.Exists(It.Is<string>(path => path.Contains("non-existent"))))
            .Returns(false);

        // Act
        var result = await _service.GetFileAsync(nonExistentPath);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be(ApplicationErrors.File.FILE_NOT_FOUND);
    }

    [Theory]
    [InlineData("test.jpg", "image/jpeg", true)]
    [InlineData("test.png", "image/png", true)]
    [InlineData("test.gif", "image/gif", true)]
    [InlineData("test.webp", "image/webp", true)]
    [InlineData("test.txt", "text/plain", false)]
    [InlineData("test.pdf", "application/pdf", false)]
    public void IsValidImageFile_ShouldValidateCorrectly(string fileName, string contentType, bool expected)
    {
        // Act
        var result = _service.IsValidImageFile(fileName, contentType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetFileUrl_ShouldReturnCorrectUrl()
    {
        // Arrange
        var filePath = "profile-images/test.png";

        // Act
        var url = _service.GetFileUrl(filePath);

        // Assert
        url.Should().Be("/test-uploads/profile-images/test.png");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testUploadPath))
        {
            Directory.Delete(_testUploadPath, true);
        }
    }
}