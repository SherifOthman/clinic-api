using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public record RestorePatientCommand(Guid Id) : IRequest<Result>;
