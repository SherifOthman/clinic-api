using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Appointments.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Appointments;

public class HandleDelayHandlerTests
{
    private readonly Mock<IDoctorSessionRepository> _sessionsMock     = new();
    private readonly Mock<IAppointmentRepository>   _appointmentsMock = new();
    private readonly Mock<IUnitOfWork>              _uowMock          = new();

    private readonly DoctorSession _lateSession = new()
    {
        DoctorInfoId = Guid.NewGuid(),
        BranchId     = Guid.NewGuid(),
        Date         = DateOnly.FromDateTime(DateTime.Today),
        CheckedInAt  = DateTimeOffset.UtcNow,
        ScheduledStartTime = new TimeOnly(8, 0),
    };

    public HandleDelayHandlerTests()
    {
        _sessionsMock
            .Setup(r => r.GetByIdAsync(_lateSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_lateSession);

        _appointmentsMock
            .Setup(r => r.GetByDoctorAndDateForUpdateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private HandleDelayHandler MakeHandler() =>
        new(_sessionsMock.Object, _appointmentsMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionNotFound()
    {
        var sessionsMock = new Mock<IDoctorSessionRepository>();
        sessionsMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DoctorSession?)null);

        var handler = new HandleDelayHandler(sessionsMock.Object, _appointmentsMock.Object, _uowMock.Object);

        var result = await handler.Handle(new HandleDelayCommand(Guid.NewGuid(), DelayHandlingOption.Manual), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Theory]
    [InlineData(DelayHandlingOption.AutoShift)]
    [InlineData(DelayHandlingOption.MarkMissed)]
    [InlineData(DelayHandlingOption.Manual)]
    public async Task Handle_ShouldFail_WhenDelayAlreadyHandled(DelayHandlingOption option)
    {
        _lateSession.DelayHandling = DelayHandlingOption.Manual;

        var result = await MakeHandler().Handle(new HandleDelayCommand(_lateSession.Id, option), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.ALREADY_EXISTS);
    }

    [Fact]
    public async Task Handle_AutoShift_ShouldShiftPendingTimeAppointments()
    {
        var session = new DoctorSession
        {
            DoctorInfoId       = Guid.NewGuid(),
            BranchId           = Guid.NewGuid(),
            Date               = DateOnly.FromDateTime(DateTime.Today),
            CheckedInAt        = DateTimeOffset.UtcNow,
            ScheduledStartTime = new TimeOnly(8, 0),
            StoredDelayMinutes = 30,
        };

        var pendingAppt = Appointment.Create(
            clinicId:             Guid.NewGuid(),
            branchId:             Guid.NewGuid(),
            patientId:            Guid.NewGuid(),
            doctorInfoId:         session.DoctorInfoId,
            visitTypeId:          Guid.NewGuid(),
            date:                 session.Date,
            type:                 AppointmentType.Time,
            scheduledTime:        new TimeOnly(9, 0),
            visitDurationMinutes: 30,
            price:                100m);
        // EndTime is set by Create() from scheduledTime + duration

        var sessionsMock = new Mock<IDoctorSessionRepository>();
        sessionsMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var apptsMock = new Mock<IAppointmentRepository>();
        apptsMock.Setup(r => r.GetByDoctorAndDateForUpdateAsync(session.DoctorInfoId, session.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pendingAppt]);

        var handler = new HandleDelayHandler(sessionsMock.Object, apptsMock.Object, _uowMock.Object);

        var result = await handler.Handle(new HandleDelayCommand(session.Id, DelayHandlingOption.AutoShift), default);

        result.IsSuccess.Should().BeTrue();
        pendingAppt.ScheduledTime.Should().Be(new TimeOnly(9, 30));
        pendingAppt.EndTime.Should().Be(new TimeOnly(10, 0));
    }

    [Fact]
    public async Task Handle_MarkMissed_ShouldMarkPastPendingAppointments_AsNoShow()
    {
        var session = new DoctorSession
        {
            DoctorInfoId       = Guid.NewGuid(),
            BranchId           = Guid.NewGuid(),
            Date               = DateOnly.FromDateTime(DateTime.Today),
            CheckedInAt        = DateTimeOffset.UtcNow,
            StoredDelayMinutes = 20,
        };

        var pastAppt = Appointment.Create(
            clinicId:             Guid.NewGuid(),
            branchId:             Guid.NewGuid(),
            patientId:            Guid.NewGuid(),
            doctorInfoId:         session.DoctorInfoId,
            visitTypeId:          Guid.NewGuid(),
            date:                 session.Date,
            type:                 AppointmentType.Time,
            scheduledTime:        new TimeOnly(0, 1),
            visitDurationMinutes: 30,
            price:                100m);

        var sessionsMock = new Mock<IDoctorSessionRepository>();
        sessionsMock.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var apptsMock = new Mock<IAppointmentRepository>();
        apptsMock.Setup(r => r.GetByDoctorAndDateForUpdateAsync(session.DoctorInfoId, session.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pastAppt]);

        var handler = new HandleDelayHandler(sessionsMock.Object, apptsMock.Object, _uowMock.Object);

        var result = await handler.Handle(new HandleDelayCommand(session.Id, DelayHandlingOption.MarkMissed), default);

        result.IsSuccess.Should().BeTrue();
        pastAppt.Status.Should().Be(AppointmentStatus.NoShow);
    }
}
