using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Events;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Aggregates;

public class AppointmentAggregateTests
{
    private static Appointment CreateTestAppointment()
    {
        return Appointment.Create(
            "APT-2024-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            1
        );
    }

    #region Creation Tests

    [Fact]
    public void Create_ValidData_ShouldCreateAppointment()
    {
        // Arrange
        var appointmentNumber = "APT-2024-001";
        var clinicBranchId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        short queueNumber = 5;

        // Act
        var appointment = Appointment.Create(
            appointmentNumber,
            clinicBranchId,
            patientId,
            doctorId,
            appointmentTypeId,
            appointmentDate,
            queueNumber
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.AppointmentNumber.Should().Be(appointmentNumber);
        appointment.ClinicBranchId.Should().Be(clinicBranchId);
        appointment.PatientId.Should().Be(patientId);
        appointment.DoctorId.Should().Be(doctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentTypeId);
        appointment.AppointmentDate.Should().Be(appointmentDate);
        appointment.QueueNumber.Should().Be(queueNumber);
        appointment.Status.Should().Be(AppointmentStatus.Pending);
        appointment.InvoiceId.Should().BeNull();
        appointment.IsConsultationFeePaid.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldRaiseAppointmentCreatedEvent()
    {
        // Act
        var appointment = CreateTestAppointment();

        // Assert
        appointment.DomainEvents.Should().ContainSingle();
        appointment.DomainEvents.Should().ContainItemsAssignableTo<AppointmentCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_InvalidAppointmentNumber_ShouldThrow(string? appointmentNumber)
    {
        // Act
        var act = () => Appointment.Create(
            appointmentNumber!,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            1
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Appointment number is required*");
    }

    [Fact]
    public void Create_EmptyClinicBranchId_ShouldThrow()
    {
        // Act
        var act = () => Appointment.Create(
            "APT-001",
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            1
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Clinic branch ID is required*");
    }

    [Fact]
    public void Create_PastDate_ShouldThrow()
    {
        // Act
        var act = () => Appointment.Create(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1),
            1
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Appointment date cannot be in the past*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidQueueNumber_ShouldThrow(short queueNumber)
    {
        // Act
        var act = () => Appointment.Create(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            queueNumber
        );

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Queue number must be positive*");
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void Confirm_FromPending_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.ClearDomainEvents();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.IsConfirmed.Should().BeTrue();
        appointment.DomainEvents.Should().ContainItemsAssignableTo<AppointmentConfirmedEvent>();
    }

    [Fact]
    public void Confirm_NotFromPending_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Confirm();

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Complete_FromConfirmed_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Confirm();
        appointment.ClearDomainEvents();

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.IsCompleted.Should().BeTrue();
        appointment.DomainEvents.Should().ContainItemsAssignableTo<AppointmentCompletedEvent>();
    }

    [Fact]
    public void Complete_NotFromConfirmed_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Cancel_FromPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.ClearDomainEvents();

        // Act
        appointment.Cancel("Patient requested");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.IsCancelled.Should().BeTrue();
        appointment.DomainEvents.Should().ContainItemsAssignableTo<AppointmentCancelledEvent>();
    }

    [Fact]
    public void Cancel_FromCompleted_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Confirm();
        appointment.Complete();

        // Act
        var act = () => appointment.Cancel();

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldBeIdempotent()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Cancel("First cancel");

        // Act
        appointment.Cancel("Second cancel");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    #endregion

    #region Reschedule Tests

    [Fact]
    public void Reschedule_ValidData_ShouldUpdateDateAndQueue()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        var newDate = DateTime.UtcNow.AddDays(5);
        short newQueue = 10;

        // Act
        appointment.Reschedule(newDate, newQueue);

        // Assert
        appointment.AppointmentDate.Should().Be(newDate);
        appointment.QueueNumber.Should().Be(newQueue);
    }

    [Fact]
    public void Reschedule_PastDate_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        var act = () => appointment.Reschedule(DateTime.UtcNow.AddDays(-1), 5);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Appointment date cannot be in the past*");
    }

    [Fact]
    public void Reschedule_CompletedAppointment_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Confirm();
        appointment.Complete();

        // Act
        var act = () => appointment.Reschedule(DateTime.UtcNow.AddDays(5), 5);

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    [Fact]
    public void Reschedule_CancelledAppointment_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        appointment.Cancel();

        // Act
        var act = () => appointment.Reschedule(DateTime.UtcNow.AddDays(5), 5);

        // Assert
        act.Should().Throw<InvalidAppointmentStateException>();
    }

    #endregion

    #region Invoice Link Tests

    [Fact]
    public void LinkInvoice_ValidInvoiceId_ShouldSetInvoiceId()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        var invoiceId = Guid.NewGuid();

        // Act
        appointment.LinkInvoice(invoiceId);

        // Assert
        appointment.InvoiceId.Should().Be(invoiceId);
    }

    [Fact]
    public void LinkInvoice_EmptyInvoiceId_ShouldThrow()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        var act = () => appointment.LinkInvoice(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Invoice ID is required*");
    }

    #endregion
}
