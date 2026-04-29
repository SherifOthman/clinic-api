using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>Doctor checks in for the day. Creates a DoctorSession and detects delays.</summary>
public record DoctorCheckInCommand(Guid DoctorInfoId, Guid BranchId) : IRequest<Result<DoctorCheckInResult>>;

public record DoctorCheckInResult(
    Guid SessionId,
    bool IsLate,
    int? DelayMinutes,
    string? ScheduledStartTime
);
