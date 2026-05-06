using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Doctor checks out for the day — ends the active session.
/// Any appointments still InProgress are automatically completed.
/// </summary>
public record DoctorCheckOutCommand(Guid DoctorInfoId, Guid BranchId) : IRequest<Result>;
