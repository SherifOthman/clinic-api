using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Moves a single appointment to a different date, optionally to a different branch.
/// Use case: doctor decides to see the last few patients tomorrow instead of today.
/// The appointment gets a new queue number on the target date; only the date (and optionally branch) changes.
/// Status resets to Pending if it was Waiting (patient hasn't been called yet).
/// </summary>
public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateOnly NewDate,
    Guid? NewBranchId = null
) : IRequest<Result>;
