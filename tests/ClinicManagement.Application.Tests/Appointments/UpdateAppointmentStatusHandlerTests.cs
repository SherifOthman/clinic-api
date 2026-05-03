using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Appointments.Commands;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Appointments;

public class UpdateAppointmentStatusHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentsMock = new();
    private readonly UpdateAppointmentStatusHandler _handler;

    public UpdateAppointmentStatusHandlerTests()
    {
        _uowMock.Setup(u => u.Appointments).Returns(_appointmentsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new UpdateAppointmentStatusHandler(_uowMock.Object);
    }

    private Appointment MakeAppointment(AppointmentStatus status) => new()
    {
        ClinicId     = Guid.NewGuid(),
        BranchId     = Guid.NewGuid(),
        PatientId    = Guid.NewGuid(),
        DoctorInfoId = Guid.NewGuid(),
        VisitTypeId  = Guid.NewGuid(),
        Date         = DateOnly.FromDateTime(DateTime.Today),
        Type         = AppointmentType.Queue,
        QueueNumber  = 1,
        Status       = status,
        IsDeleted    = false,
    };

    [Theory]
    [InlineData(AppointmentStatus.Pending,    AppointmentStatus.Waiting,    true)]
    [InlineData(AppointmentStatus.Pending,    AppointmentStatus.InProgress, true)]
    [InlineData(AppointmentStatus.Pending,    AppointmentStatus.Cancelled,  true)]
    [InlineData(AppointmentStatus.Pending,    AppointmentStatus.NoShow,     true)]
    [InlineData(AppointmentStatus.Waiting,    AppointmentStatus.InProgress, true)]
    [InlineData(AppointmentStatus.Waiting,    AppointmentStatus.Cancelled,  true)]
    [InlineData(AppointmentStatus.Waiting,    AppointmentStatus.NoShow,     true)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Completed,  true)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Cancelled,  true)]
    [InlineData(AppointmentStatus.Completed,  AppointmentStatus.Pending,    false)]
    [InlineData(AppointmentStatus.Cancelled,  AppointmentStatus.Pending,    false)]
    [InlineData(AppointmentStatus.NoShow,     AppointmentStatus.Pending,    false)]
    public async Task Handle_ShouldRespectValidTransitions(
        AppointmentStatus from, AppointmentStatus to, bool shouldSucceed)
    {
        var appt = MakeAppointment(from);
        _appointmentsMock.Setup(x => x.GetByIdForUpdateAsync(appt.Id, default)).ReturnsAsync(appt);

        var result = await _handler.Handle(new UpdateAppointmentStatusCommand(appt.Id, to), default);

        if (shouldSucceed)
            result.IsSuccess.Should().BeTrue();
        else
        {
            result.IsFailure.Should().BeTrue();
            result.ErrorCode.Should().Be(ErrorCodes.OPERATION_NOT_ALLOWED);
        }
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAppointmentNotFound()
    {
        var id = Guid.NewGuid();
        _appointmentsMock.Setup(x => x.GetByIdForUpdateAsync(id, default)).ReturnsAsync((Appointment?)null);

        var result = await _handler.Handle(
            new UpdateAppointmentStatusCommand(id, AppointmentStatus.InProgress), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }
}
