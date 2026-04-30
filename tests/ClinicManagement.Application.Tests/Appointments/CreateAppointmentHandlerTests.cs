using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Appointments.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Appointments;

public class CreateAppointmentHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CreateAppointmentHandler _handler;

    private readonly Guid _clinicId  = Guid.NewGuid();
    private readonly Guid _branchId  = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _doctorId  = Guid.NewGuid();
    private readonly VisitType _visitType;

    public CreateAppointmentHandlerTests()
    {
        _visitType = new VisitType
        {
            Name = "Consultation",
            Price = 100, IsActive = true,
        };

        var apptRepoMock    = new Mock<IAppointmentRepository>();
        var queueCounterMock = new Mock<IQueueCounterRepository>();
        var scheduleRepoMock = new Mock<IDoctorScheduleRepository>();

        // Visit type found
        scheduleRepoMock
            .Setup(r => r.GetVisitTypeByIdAsync(_visitType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_visitType);

        // Visit type not found for unknown IDs
        scheduleRepoMock
            .Setup(r => r.GetVisitTypeByIdAsync(It.Is<Guid>(id => id != _visitType.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VisitType?)null);

        // Queue counter returns 1
        queueCounterMock
            .Setup(r => r.NextAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Time slot not taken
        apptRepoMock
            .Setup(r => r.TimeSlotTakenAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        apptRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);
        _uowMock.Setup(u => u.QueueCounters).Returns(queueCounterMock.Object);
        _uowMock.Setup(u => u.DoctorSchedules).Returns(scheduleRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);

        _handler = new CreateAppointmentHandler(_uowMock.Object, _currentUserMock.Object);
    }

    private CreateAppointmentCommand QueueCmd(Guid? visitTypeId = null) =>
        new(_branchId, _patientId, _doctorId, visitTypeId ?? _visitType.Id,
            DateOnly.FromDateTime(DateTime.Today), AppointmentType.Queue, null, null);

    private CreateAppointmentCommand TimeCmd(TimeOnly? time = null) =>
        new(_branchId, _patientId, _doctorId, _visitType.Id,
            DateOnly.FromDateTime(DateTime.Today), AppointmentType.Time, time, null);

    [Fact]
    public async Task Handle_Queue_ShouldSucceedAndAssignQueueNumber()
    {
        var result = await _handler.Handle(QueueCmd(), default);

        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(u => u.QueueCounters.NextAsync(_doctorId, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Time_ShouldFail_WhenNoScheduledTime()
    {
        var result = await _handler.Handle(TimeCmd(null), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.VALIDATION_ERROR);
    }

    [Fact]
    public async Task Handle_Time_ShouldSucceed_WhenTimeProvided()
    {
        var result = await _handler.Handle(TimeCmd(new TimeOnly(10, 0)), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVisitTypeNotFound()
    {
        var result = await _handler.Handle(QueueCmd(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Time_ShouldFail_WhenSlotAlreadyTaken()
    {
        var apptRepoMock = new Mock<IAppointmentRepository>();
        apptRepoMock
            .Setup(r => r.TimeSlotTakenAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);

        var result = await _handler.Handle(TimeCmd(new TimeOnly(10, 0)), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.CONFLICT);
    }

    [Fact]
    public async Task Handle_ShouldApplyDiscount_AndCalculateCorrectFinalPrice()
    {
        Appointment? savedAppt = null;
        var apptRepoMock = new Mock<IAppointmentRepository>();
        apptRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => savedAppt = a)
            .Returns(Task.CompletedTask);
        apptRepoMock
            .Setup(r => r.TimeSlotTakenAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);

        var cmd = new CreateAppointmentCommand(
            _branchId, _patientId, _doctorId, _visitType.Id,
            DateOnly.FromDateTime(DateTime.Today),
            AppointmentType.Queue, null, 10m); // 10% discount on price 100

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        savedAppt.Should().NotBeNull();
        savedAppt!.FinalPrice.Should().Be(90m); // 100 - 10% = 90
        savedAppt.DiscountPercent.Should().Be(10m);
    }
}
