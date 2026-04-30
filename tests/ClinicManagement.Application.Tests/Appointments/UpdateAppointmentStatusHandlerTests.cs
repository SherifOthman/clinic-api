using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Appointments.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Appointments;

public class UpdateAppointmentStatusHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly UpdateAppointmentStatusHandler _handler;

    public UpdateAppointmentStatusHandlerTests()
    {
        _handler = new UpdateAppointmentStatusHandler(_uow);
    }

    private async Task<Appointment> SeedAppointmentAsync(AppointmentStatus status = AppointmentStatus.Pending)
    {
        var clinic  = TestHandlerHelpers.CreateTestClinic();
        var branch  = TestHandlerHelpers.CreateTestBranch(clinic.Id);
        var patient = TestHandlerHelpers.CreateTestPatient(clinicId: clinic.Id);
        await _uow.Clinics.AddAsync(clinic);
        await _uow.Branches.AddAsync(branch);
        await _uow.Patients.AddAsync(patient);

        var appt = new Appointment
        {
            ClinicId     = clinic.Id,
            BranchId     = branch.Id,
            PatientId    = patient.Id,
            DoctorInfoId = Guid.NewGuid(),
            VisitTypeId  = Guid.NewGuid(),
            Date         = DateOnly.FromDateTime(DateTime.Today),
            Type         = AppointmentType.Queue,
            QueueNumber  = 1,
            Status       = status,
        };
        appt.ApplyPrice(100);
        await _uow.Appointments.AddAsync(appt);
        await _uow.SaveChangesAsync();
        return appt;
    }

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
        var appt = await SeedAppointmentAsync(from);
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
        var result = await _handler.Handle(
            new UpdateAppointmentStatusCommand(Guid.NewGuid(), AppointmentStatus.InProgress), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }
}
