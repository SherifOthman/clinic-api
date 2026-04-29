using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>Clinic owner sets whether a doctor uses Queue or Time appointments.</summary>
public record SetDoctorAppointmentTypeCommand(Guid MemberId, AppointmentType AppointmentType) : IRequest<Result>;
