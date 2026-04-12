using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record DeletePatientCommand(Guid Id) : IRequest<Result>;
