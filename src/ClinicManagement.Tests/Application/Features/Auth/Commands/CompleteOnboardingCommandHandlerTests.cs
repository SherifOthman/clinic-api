using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class CompleteOnboardingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILocationsService> _locationsServiceMock;
    private readonly Mock<ILogger<CompleteOnboardingCommandHandler>> _loggerMock;
    private readonly CompleteOnboardingCommandHandler _handler;

    public CompleteOnboardingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _locationsServiceMock = new Mock<ILocationsService>();
        _loggerMock = new Mock<ILogger<CompleteOnboardingCommandHandler>>();
        _handler = new CompleteOnboardingCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _locationsServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnError()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        var command = new CompleteOnboardingCommand(
            "Test Clinic", 1, "Main Branch", "123 Main St", 1, 2, 3, new List<BranchPhoneNumberDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authentication.USER_NOT_AUTHENTICATED, result.Code);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateClinicAndReturnId()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);
        var subscriptionPlan = new SubscriptionPlan { Id = 1, IsActive = true };

        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = userId; return true; });

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var subscriptionRepoMock = new Mock<ISubscriptionPlanRepository>();
        subscriptionRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptionPlan);

        var clinicRepoMock = new Mock<IClinicRepository>();
        var branchRepoMock = new Mock<IRepository<ClinicBranch>>();
        var phoneRepoMock = new Mock<IRepository<ClinicBranchPhoneNumber>>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(subscriptionRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Clinics).Returns(clinicRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.ClinicBranches).Returns(branchRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.ClinicBranchPhoneNumbers).Returns(phoneRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _locationsServiceMock.Setup(x => x.GetCountriesAsync())
            .ReturnsAsync(new List<CountryDto> { new CountryDto { Id = 1, Name = "USA", Code = "US" } });
        _locationsServiceMock.Setup(x => x.GetCitiesAsync(1, 2))
            .ReturnsAsync(new List<CityDto> { new CityDto { Id = 3, Name = "New York" } });

        var command = new CompleteOnboardingCommand(
            "Test Clinic", 1, "Main Branch", "123 Main St", 1, 2, 3, new List<BranchPhoneNumberDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(1));
    }

    private static User CreateTestUser(int id)
    {
        return new User
        {
            Id = id,
            UserName = $"user{id}@test.com",
            Email = $"user{id}@test.com",
            FirstName = "Test",
            LastName = "User",
            ClinicId = null
        };
    }
}