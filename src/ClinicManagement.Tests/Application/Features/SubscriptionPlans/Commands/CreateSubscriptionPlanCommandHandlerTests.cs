using ClinicManagement.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.SubscriptionPlans.Commands;

public class CreateSubscriptionPlanCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<CreateSubscriptionPlanCommandHandler>> _loggerMock;
    private readonly CreateSubscriptionPlanCommandHandler _handler;

    public CreateSubscriptionPlanCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<CreateSubscriptionPlanCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);
        
        _handler = new CreateSubscriptionPlanCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSubscriptionPlan()
    {
        // Arrange
        var command = new CreateSubscriptionPlanCommand
        {
            Name = "Premium Plan",
            Description = "Premium subscription with all features",
            Price = 99.99m,
            MaxUsers = 10,
            MaxPatients = 1000
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Premium Plan");
        result.Value.Price.Should().Be(99.99m);
        
        _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(
            It.Is<SubscriptionPlan>(p => p.Name == "Premium Plan" && p.Price == 99.99m), 
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateSubscriptionPlanCommand
        {
            Name = "Test Plan",
            Price = 50.00m
        };

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }
}