using ClinicManagement.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.SubscriptionPlans.Commands;

public class UpdateSubscriptionPlanCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<UpdateSubscriptionPlanCommandHandler>> _loggerMock;
    private readonly UpdateSubscriptionPlanCommandHandler _handler;

    public UpdateSubscriptionPlanCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<UpdateSubscriptionPlanCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);
        
        _handler = new UpdateSubscriptionPlanCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSubscriptionPlan()
    {
        // Arrange
        var existingPlan = new SubscriptionPlan
        {
            Id = 1,
            Name = "Basic Plan",
            Price = 29.99m,
            IsActive = true
        };

        var command = new UpdateSubscriptionPlanCommand
        {
            Id = 1,
            Name = "Updated Basic Plan",
            Price = 39.99m,
            MaxUsers = 5
        };

        _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Updated Basic Plan");
        result.Value.Price.Should().Be(39.99m);
        
        _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(existingPlan, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPlanNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateSubscriptionPlanCommand { Id = 999 };
        
        _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Subscription plan not found");
    }
}