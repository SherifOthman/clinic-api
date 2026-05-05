using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Sets the appointment type (Queue/Time) for a doctor at a specific branch.
/// Allowed by: clinic owner, or the doctor themselves when CanSelfManageSchedule is true.
/// </summary>
public record SetDoctorAppointmentTypeCommand(
    Guid MemberId,
    Guid BranchId,
    AppointmentType AppointmentType) : IRequest<Result>;
