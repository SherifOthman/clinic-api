using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record SetOwnerAsDoctorCommand(Guid SpecializationId) : IRequest<Result>;
