using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Reschedules all pending/waiting appointments for a doctor from a given date onwards.
///
/// Use case: doctor is absent today (or for multiple days) and the clinic needs to
/// move all affected appointments to the next available working days.
///
/// Strategy:
///   - Finds all pending/waiting appointments from FromDate onwards.
///   - Groups them by their original date.
///   - For each group, finds the next available working day (skipping dates that
///     already have appointments or are in the past).
///   - Moves the appointments to the new date, preserving relative order.
///   - For Time-based: keeps the same time slots on the new date.
///   - For Queue-based: keeps the same queue numbers on the new date.
///
/// Returns the number of appointments rescheduled.
/// </summary>
public record RescheduleDoctorCommand(
    Guid DoctorInfoId,
    Guid BranchId,
    DateOnly FromDate
) : IRequest<Result<int>>;
