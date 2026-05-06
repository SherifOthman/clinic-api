using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Moves a single appointment to a different date.
/// Use case: doctor decides to see the last few patients tomorrow instead of today.
/// The appointment keeps its queue number and visit type; only the date changes.
/// Status resets to Pending if it was Waiting (patient hasn't been called yet).
/// </summary>
public record RescheduleAppointmentCommand(Guid AppointmentId, DateOnly NewDate) : IRequest<Result>;
