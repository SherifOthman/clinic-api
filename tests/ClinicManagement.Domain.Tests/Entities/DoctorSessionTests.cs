using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class DoctorSessionTests
{
    private static readonly DateOnly Today = new(2026, 1, 15);

    // The entity uses LocalDateTime for delay calculation, so we must construct
    // CheckedInAt in local time to avoid timezone offset skewing the minutes.
    private static DoctorSession MakeSession(
        TimeOnly? scheduledStart,
        TimeOnly? checkedInAt)
    {
        DateTimeOffset? checkedInOffset = checkedInAt.HasValue
            ? new DateTimeOffset(Today.ToDateTime(checkedInAt.Value), TimeZoneInfo.Local.GetUtcOffset(Today.ToDateTime(checkedInAt.Value)))
            : null;

        return new DoctorSession
        {
            ClinicId           = Guid.NewGuid(),
            DoctorInfoId       = Guid.NewGuid(),
            BranchId           = Guid.NewGuid(),
            Date               = Today,
            ScheduledStartTime = scheduledStart,
            CheckedInAt        = checkedInOffset,
        };
    }

    // ── IsActive ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenCheckedInAndNotCheckedOut()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 0));
        session.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenNotCheckedIn()
    {
        var session = MakeSession(new TimeOnly(9, 0), null);
        session.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenCheckedOut()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 0));
        session.CheckedOutAt = DateTimeOffset.UtcNow;
        session.IsActive.Should().BeFalse();
    }

    // ── DelayMinutes ──────────────────────────────────────────────────────────

    [Fact]
    public void DelayMinutes_ShouldReturnNull_WhenNotCheckedIn()
    {
        var session = MakeSession(new TimeOnly(9, 0), null);
        session.DelayMinutes.Should().BeNull();
    }

    [Fact]
    public void DelayMinutes_ShouldReturnNull_WhenNoScheduledTime()
    {
        var session = MakeSession(null, new TimeOnly(9, 0));
        session.DelayMinutes.Should().BeNull();
    }

    [Fact]
    public void DelayMinutes_ShouldReturnNull_WhenOnTime()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 0));
        session.DelayMinutes.Should().BeNull();
    }

    [Fact]
    public void DelayMinutes_ShouldReturnNull_WhenEarly()
    {
        // Arrived 15 minutes early — not a delay
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(8, 45));
        session.DelayMinutes.Should().BeNull();
    }

    [Fact]
    public void DelayMinutes_ShouldReturnPositiveValue_WhenLate()
    {
        // Arrived 30 minutes late
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 30));
        session.DelayMinutes.Should().Be(30);
    }

    [Fact]
    public void DelayMinutes_ShouldReturnCorrectValue_ForLargeDelay()
    {
        // Arrived 2 hours late
        var session = MakeSession(new TimeOnly(8, 0), new TimeOnly(10, 0));
        session.DelayMinutes.Should().Be(120);
    }

    // ── IsLate ────────────────────────────────────────────────────────────────

    [Fact]
    public void IsLate_ShouldReturnFalse_WhenOnTime()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 0));
        session.IsLate.Should().BeFalse();
    }

    [Fact]
    public void IsLate_ShouldReturnFalse_WhenEarly()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(8, 50));
        session.IsLate.Should().BeFalse();
    }

    [Fact]
    public void IsLate_ShouldReturnTrue_WhenLate()
    {
        var session = MakeSession(new TimeOnly(9, 0), new TimeOnly(9, 15));
        session.IsLate.Should().BeTrue();
    }

    [Fact]
    public void IsLate_ShouldReturnFalse_WhenNotCheckedIn()
    {
        var session = MakeSession(new TimeOnly(9, 0), null);
        session.IsLate.Should().BeFalse();
    }
}
