using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Receptionist chooses how to handle a doctor's delay.
/// Options: AutoShift (move all forward), MarkMissed (mark past as NoShow), Manual (do nothing).
/// </summary>
public record HandleDelayCommand(Guid SessionId, DelayHandlingOption Option) : IRequest<Result>;
