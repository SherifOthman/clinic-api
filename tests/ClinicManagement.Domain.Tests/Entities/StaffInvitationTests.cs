using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Tests.Builders;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class StaffInvitationTests
{
    [Fact]
    public void Create_ShouldInitializeWithCorrectDefaults()
    {
        var invitation = StaffInvitation.Create(Guid.NewGuid(), "doc@test.com", ClinicMemberRole.Doctor, Guid.NewGuid());

        invitation.IsAccepted.Should().BeFalse();
        invitation.IsCanceled.Should().BeFalse();
        invitation.InvitationToken.Should().NotBeNullOrWhiteSpace();
        invitation.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow.AddDays(6));
    }

    [Fact]
    public void Accept_ShouldSucceed_WhenInvitationIsValid()
    {
        var invitation = StaffInvitationBuilder.New().Build();
        var userId = Guid.NewGuid();
        var acceptedAt = DateTimeOffset.UtcNow;

        var result = invitation.Accept(userId, acceptedAt);

        result.IsSuccess.Should().BeTrue();
        invitation.IsAccepted.Should().BeTrue();
        invitation.AcceptedByUserId.Should().Be(userId);
        invitation.AcceptedAt.Should().Be(acceptedAt);
    }

    [Theory]
    [InlineData("Accepted")]
    [InlineData("Canceled")]
    [InlineData("Expired")]
    public void Accept_ShouldFail_WhenInvitationIsNotPending(string state)
    {
        var invitation = state == "Expired"
            ? StaffInvitationBuilder.New().Expired().Build()
            : StaffInvitationBuilder.New().Build();

        if (state == "Accepted") invitation.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);
        if (state == "Canceled") invitation.Cancel();

        var result = invitation.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenInvitationIsPending()
    {
        var invitation = StaffInvitationBuilder.New().Build();

        var result = invitation.Cancel();

        result.IsSuccess.Should().BeTrue();
        invitation.IsCanceled.Should().BeTrue();
    }

    [Theory]
    [InlineData("Accepted")]
    [InlineData("Canceled")]
    public void Cancel_ShouldFail_WhenInvitationIsNotPending(string state)
    {
        var invitation = StaffInvitationBuilder.New().Build();
        if (state == "Accepted") invitation.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);
        if (state == "Canceled") invitation.Cancel();

        var result = invitation.Cancel();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenPastExpiryDate()
    {
        var invitation = StaffInvitationBuilder.New().Expired().Build();
        invitation.IsExpired(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenBeforeExpiryDate()
    {
        var invitation = StaffInvitationBuilder.New().Build();
        invitation.IsExpired(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenPendingAndNotExpired()
    {
        var invitation = StaffInvitationBuilder.New().Build();
        invitation.IsValid(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Theory]
    [InlineData("Accepted")]
    [InlineData("Canceled")]
    [InlineData("Expired")]
    public void IsValid_ShouldReturnFalse_WhenNotPending(string state)
    {
        var invitation = state == "Expired"
            ? StaffInvitationBuilder.New().Expired().Build()
            : StaffInvitationBuilder.New().Build();

        if (state == "Accepted") invitation.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);
        if (state == "Canceled") invitation.Cancel();

        invitation.IsValid(DateTimeOffset.UtcNow).Should().BeFalse();
    }
}
