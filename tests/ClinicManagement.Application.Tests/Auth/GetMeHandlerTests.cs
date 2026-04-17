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
    public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var result = await _handler.Handle(new GetMeQuery(Guid.NewGuid()), default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnUserData_WithGenderMapped()
    {
        var user = TestHandlerHelpers.CreateTestUser("ahmed@test.com");
        user.UserName = "ahmed.ali";
        user.Person.FirstName = "Ahmed";
        user.Person.LastName  = "Ali";
        user.Person.Gender    = Gender.Male;
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMeQuery(user.Id), default);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Ahmed");
        result.Gender.Should().Be("Male");
        result.OnboardingCompleted.Should().BeFalse();
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

        result!.OnboardingCompleted.Should().BeTrue();
    }
}
