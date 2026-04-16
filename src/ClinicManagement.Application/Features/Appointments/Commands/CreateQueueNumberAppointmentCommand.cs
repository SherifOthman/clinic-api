using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public record CreateQueueNumberAppointmentCommand(
    Guid ClinicBranchId,
    Guid PatientId,
    Guid DoctorId,
    DateOnly Date,
    Guid DoctorVisitTypeId,
    decimal? DiscountPercent) : IRequest<Result<Guid>>;
