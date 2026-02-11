using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using FluentAssertions;

namespace ClinicManagement.Tests.Domain;

public class AppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        var appointmentNumber = "APT-2026-000001";
        var clinicBranchId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        short queueNumber = 1;

        // Act
        var appointment = Appointment.Create(
            appointmentNumber,
            clinicBranchId,
            patientId,
            doctorId,
            appointmentTypeId,
            appointmentDate,
            queueNumber);

        // Assert
        appointment.Should().NotBeNull();
        appointment.AppointmentNumber.Should().Be(appointmentNumber);
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.IsPending.Should().BeTrue();
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => Appointment.Create(
            "APT-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            pastDate,
            1);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*past*");
    }

    [Fact]
    public void Confirm_WhenPending_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.IsConfirmed.Should().BeTrue();
    }

    [Fact]
    public void Confirm_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm(); // Already confirmed

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Complete_WhenConfirmed_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Complete_WhenNotConfirmed_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowException()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.Complete();

        // Act
        var act = () => appointment.Cancel();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    private static Appointment CreateValidAppointment()
    {
        return Appointment.Create(
            "APT-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            1);
    }
}
