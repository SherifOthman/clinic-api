using ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.ChronicDiseases.Commands;

public class CreateChronicDiseaseCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IChronicDiseaseRepository> _chronicDiseaseRepositoryMock;
    private readonly Mock<ILogger<CreateChronicDiseaseCommandHandler>> _loggerMock;
    private readonly CreateChronicDiseaseCommandHandler _handler;

    public CreateChronicDiseaseCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _chronicDiseaseRepositoryMock = new Mock<IChronicDiseaseRepository>();
        _loggerMock = new Mock<ILogger<CreateChronicDiseaseCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.ChronicDiseases).Returns(_chronicDiseaseRepositoryMock.Object);
        
        _handler = new CreateChronicDiseaseCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateChronicDisease()
    {
        // Arrange
        var command = new CreateChronicDiseaseCommand
        {
            Name = "Diabetes Type 2",
            Description = "Type 2 Diabetes Mellitus"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Diabetes Type 2");
        result.Value.Description.Should().Be("Type 2 Diabetes Mellitus");
        
        _chronicDiseaseRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ChronicDisease>(d => d.Name == "Diabetes Type 2" && d.IsActive), 
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateChronicDiseaseCommand
        {
            Name = "Test Disease"
        };

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
}