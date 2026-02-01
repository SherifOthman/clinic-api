using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand : IRequest<Result>
{
    public Guid Id { get; init; }
}
