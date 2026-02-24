
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Tests.Builders;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class StaffInvitationTests
{
    public enum InvitationState
    {
        Accepted,
        Canceled,
        Expired
    }

    [Fact]
    public void Create_ShouldInitializeInvitationCorrectly()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var email = "doctor@test.com";
        var role = "Doctor";
        var createdByUserId = Guid.NewGuid();

        // Act
        var invitation = StaffInvitation.Create(
            clinicId,
            email,
            role,
            createdByUserId);

        // Assert

        invitation.ClinicId.Should().Be(clinicId);
        invitation.Email.Should().Be(email);
        invitation.Role.Should().Be(role);

        invitation.IsAccepted.Should().BeFalse();
        invitation.IsCanceled.Should().BeFalse();

        invitation.InvitationToken.Should().NotBeNullOrWhiteSpace();
        invitation.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(6));
    }

    [Fact]
    public void Accept_ShouldMarkInvitationAsAccepted_WhenInvitationIsValid()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        var userId = Guid.NewGuid();
        var acceptedAt = DateTime.UtcNow;

        // Act
        var result = invitation.Accept(userId, acceptedAt);

        // Assert
        result.IsSuccess.Should().BeTrue();

        invitation.IsAccepted.Should().BeTrue();
        invitation.AcceptedByUserId.Should().Be(userId);
        invitation.AcceptedAt.Should().Be(acceptedAt);
    }


    [Theory]
    [InlineData(InvitationState.Accepted)]
    [InlineData(InvitationState.Canceled)]
    [InlineData(InvitationState.Expired)]
    public void Accept_ShouldFail_WhenInvitationIsInvalid(InvitationState state)
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        switch (state)
        {
            case InvitationState.Accepted:
                invitation.Accept(Guid.NewGuid(), DateTime.UtcNow);
                break;
            case InvitationState.Canceled:
                invitation.Cancel();
                break;
            case InvitationState.Expired:
                invitation = StaffInvitation.Create(
                    Guid.NewGuid(),
                    "doctor@test.com",
                    "Doctor",
                    Guid.NewGuid(),
                    null,
                    -1);
                break;
        }

        var userId = Guid.NewGuid();
        var acceptedAt = DateTime.UtcNow;

        // Act
        var result = invitation.Accept(userId, acceptedAt);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldMarkInvitationAsCanceled_WhenInvitationIsValid()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        invitation.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenInvitationIsAlreadyAccepted()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        invitation.Accept(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldFail_WhenInvitationIsAlreadyCanceled()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        invitation.Cancel();

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenCurrentTimeIsAfterExpiration()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Expired().Build();
       
        var now = DateTime.UtcNow;

        // Act
        var result = invitation.IsExpired(now);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenCurrentTimeIsBeforeExpiration()
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        // Act
        var result = invitation.IsExpired(DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenInvitationIsValid()
    {
        // Arrange
        var invitation = StaffInvitation.Create(
            Guid.NewGuid(),
            "doctor@test.com",
            "Doctor",
            Guid.NewGuid());

        var now = DateTime.UtcNow;

        // Act
        var result = invitation.IsValid(now);

        // Assert
        result.Should().BeTrue();
    }


    [Theory]
    [InlineData(InvitationState.Accepted)]
    [InlineData(InvitationState.Canceled)]
    [InlineData(InvitationState.Expired)]
    public void IsValid_ShouldReturnFalse_ForInvalidStates(InvitationState state)
    {
        // Arrange
        var invitation = StaffInvitationBuilder.New().Build();

        var now = DateTime.UtcNow;

        switch (state)
        {
            case InvitationState.Accepted:
                invitation.Accept(Guid.NewGuid(), now);
                break;

            case InvitationState.Canceled:
                invitation.Cancel();
                break;

            case InvitationState.Expired:
                invitation = StaffInvitation.Create(
                    Guid.NewGuid(),
                    "doctor@test.com",
                    "Doctor",
                    Guid.NewGuid(),
                                        null,
                    -1);
                break;

        }

        // Act 
        var result = invitation.IsValid(now);

        // Assert
        result.Should().BeFalse();
    }

}