using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Cancels all pending/waiting appointments for a doctor on a specific date.
/// Use case: doctor can't come today — receptionist cancels all their appointments at once.
/// Only Pending and Waiting appointments are cancelled; InProgress/Completed are left untouched.
/// </summary>
public record BulkCancelAppointmentsCommand(
    Guid DoctorInfoId,
    Guid BranchId,
    DateOnly Date
) : IRequest<Result<int>>;
