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
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly HandleDelayHandler _handler;

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
        var sessionRepoMock = new Mock<IDoctorSessionRepository>();
        var apptRepoMock    = new Mock<IAppointmentRepository>();

        sessionRepoMock
            .Setup(r => r.GetByIdAsync(_lateSession.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_lateSession);

        apptRepoMock
            .Setup(r => r.GetByDoctorAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _uowMock.Setup(u => u.DoctorSessions).Returns(sessionRepoMock.Object);
        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new HandleDelayHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSessionNotFound()
    {
        var sessionRepoMock = new Mock<IDoctorSessionRepository>();
        sessionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DoctorSession?)null);
        _uowMock.Setup(u => u.DoctorSessions).Returns(sessionRepoMock.Object);

        var result = await _handler.Handle(new HandleDelayCommand(Guid.NewGuid(), DelayHandlingOption.Manual), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Theory]
    [InlineData(DelayHandlingOption.AutoShift)]
    [InlineData(DelayHandlingOption.MarkMissed)]
    [InlineData(DelayHandlingOption.Manual)]
    public async Task Handle_ShouldSucceed_WhenDelayAlreadyHandled_ReturnsFail(DelayHandlingOption option)
    {
        // Arrange: session that already has a handling decision
        _lateSession.DelayHandling = DelayHandlingOption.Manual;

        var result = await _handler.Handle(new HandleDelayCommand(_lateSession.Id, option), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.ALREADY_EXISTS);
    }

    [Fact]
    public async Task Handle_AutoShift_ShouldShiftPendingTimeAppointments_ByDelayMinutes()
    {
        // Arrange: a session with a known stored delay
        var session = new DoctorSession
        {
            DoctorInfoId       = Guid.NewGuid(),
            BranchId           = Guid.NewGuid(),
            Date               = DateOnly.FromDateTime(DateTime.Today),
            CheckedInAt        = DateTimeOffset.UtcNow,
            ScheduledStartTime = new TimeOnly(8, 0),
            StoredDelayMinutes = 30,
        };

        var pendingAppt = new Appointment
        {
            DoctorInfoId  = session.DoctorInfoId,
            Date          = session.Date,
            Type          = AppointmentType.Time,
            Status        = AppointmentStatus.Pending,
            ScheduledTime = new TimeOnly(9, 0),
            EndTime       = new TimeOnly(9, 30),
        };
        pendingAppt.ApplyPrice(100);

        var sessionRepoMock = new Mock<IDoctorSessionRepository>();
        sessionRepoMock
            .Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var apptRepoMock = new Mock<IAppointmentRepository>();
        apptRepoMock
            .Setup(r => r.GetByDoctorAndDateAsync(session.DoctorInfoId, session.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pendingAppt]);

        _uowMock.Setup(u => u.DoctorSessions).Returns(sessionRepoMock.Object);
        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);

        var handler = new HandleDelayHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new HandleDelayCommand(session.Id, DelayHandlingOption.AutoShift), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pendingAppt.ScheduledTime.Should().Be(new TimeOnly(9, 30)); // shifted +30 min
        pendingAppt.EndTime.Should().Be(new TimeOnly(10, 0));       // shifted +30 min
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

        // An appointment scheduled in the past (before now)
        var pastAppt = new Appointment
        {
            DoctorInfoId  = session.DoctorInfoId,
            Date          = session.Date,
            Type          = AppointmentType.Time,
            Status        = AppointmentStatus.Pending,
            ScheduledTime = new TimeOnly(0, 1), // 00:01 — always in the past
        };
        pastAppt.ApplyPrice(100);

        var sessionRepoMock = new Mock<IDoctorSessionRepository>();
        sessionRepoMock
            .Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var apptRepoMock = new Mock<IAppointmentRepository>();
        apptRepoMock
            .Setup(r => r.GetByDoctorAndDateAsync(session.DoctorInfoId, session.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([pastAppt]);

        _uowMock.Setup(u => u.DoctorSessions).Returns(sessionRepoMock.Object);
        _uowMock.Setup(u => u.Appointments).Returns(apptRepoMock.Object);

        var handler = new HandleDelayHandler(_uowMock.Object);

        var result = await handler.Handle(new HandleDelayCommand(session.Id, DelayHandlingOption.MarkMissed), default);

        result.IsSuccess.Should().BeTrue();
        pastAppt.Status.Should().Be(AppointmentStatus.NoShow);
    }
}
