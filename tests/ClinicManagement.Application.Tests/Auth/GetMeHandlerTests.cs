using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Auth.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Auth;

public class GetMeHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetMeHandler _handler;

    public GetMeHandlerTests()
    {
        _handler = new GetMeHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        var result = await _handler.Handle(new GetMeQuery(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnUserData_WithGenderMapped()
    {
        var user = TestHandlerHelpers.CreateTestUser("ahmed@test.com");
        user.UserName = "ahmed.ali";
        user.FullName = "Ahmed Ali";
        user.Gender = Gender.Male;
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMeQuery(user.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FullName.Should().Be("Ahmed Ali");
        result.Value.Gender.Should().Be("Male");
        result.Value.OnboardingCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnOnboardingCompleted_WhenClinicExists()
    {
        var user = TestHandlerHelpers.CreateTestUser("owner@test.com");
        user.UserName = "owner";
        _uow.UserEntities.Add(user);
        var plan = TestHandlerHelpers.CreateTestSubscriptionPlan();
        await _uow.SubscriptionPlans.AddAsync(plan);
        await _uow.SaveChangesAsync();

        await _uow.Clinics.AddAsync(TestHandlerHelpers.CreateTestClinic(ownerUserId: user.Id, subscriptionPlanId: plan.Id));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMeQuery(user.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OnboardingCompleted.Should().BeTrue();
    }
}
