using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class AppointmentTests
{
    private static Appointment Make() => new()
    {
        ClinicId     = Guid.NewGuid(),
        BranchId     = Guid.NewGuid(),
        PatientId    = Guid.NewGuid(),
        DoctorInfoId = Guid.NewGuid(),
        VisitTypeId  = Guid.NewGuid(),
        Date         = DateOnly.FromDateTime(DateTime.Today),
    };

    // ── ApplyPrice — no discount ──────────────────────────────────────────────

    [Fact]
    public void ApplyPrice_ShouldSetPrice()
    {
        var appt = Make();
        appt.ApplyPrice(150m);
        appt.Price.Should().Be(150m);
    }

    [Fact]
    public void ApplyPrice_ShouldSetFinalPriceEqualToPrice_WhenNoDiscount()
    {
        var appt = Make();
        appt.ApplyPrice(150m);
        appt.FinalPrice.Should().Be(150m);
        appt.DiscountPercent.Should().BeNull();
    }

    // ── ApplyPrice — with discount ────────────────────────────────────────────

    [Fact]
    public void ApplyPrice_ShouldApplyDiscount_WhenDiscountProvided()
    {
        var appt = Make();
        appt.ApplyPrice(200m, 10m); // 10% off 200 = 180
        appt.FinalPrice.Should().Be(180m);
    }

    [Fact]
    public void ApplyPrice_ShouldStoreDiscountPercent()
    {
        var appt = Make();
        appt.ApplyPrice(200m, 10m);
        appt.DiscountPercent.Should().Be(10m);
    }

    [Theory]
    [InlineData(100,  0,  100)]   // 0% discount
    [InlineData(100, 25,   75)]   // 25% off
    [InlineData(100, 50,   50)]   // 50% off
    [InlineData(100, 100,   0)]   // 100% off (free)
    [InlineData(199, 10, 179.10)] // rounding check
    public void ApplyPrice_ShouldCalculateFinalPrice_Correctly(
        decimal price, decimal discount, decimal expected)
    {
        var appt = Make();
        appt.ApplyPrice(price, discount);
        appt.FinalPrice.Should().Be(expected);
    }

    [Fact]
    public void ApplyPrice_ShouldRoundToTwoDecimalPlaces()
    {
        var appt = Make();
        appt.ApplyPrice(100m, 33m); // 100 * 0.67 = 67.00 exactly
        appt.FinalPrice.Should().Be(Math.Round(100m * (1 - 33m / 100m), 2));
    }

    // ── ApplyPrice — overwrite ────────────────────────────────────────────────

    [Fact]
    public void ApplyPrice_ShouldOverwritePreviousValues_WhenCalledAgain()
    {
        var appt = Make();
        appt.ApplyPrice(100m, 10m);
        appt.ApplyPrice(200m);       // no discount this time

        appt.Price.Should().Be(200m);
        appt.FinalPrice.Should().Be(200m);
        appt.DiscountPercent.Should().BeNull();
    }
}
