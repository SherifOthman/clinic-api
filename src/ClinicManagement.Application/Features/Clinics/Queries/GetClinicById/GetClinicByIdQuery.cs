using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinicById;

public record GetClinicByIdQuery : IRequest<Result<ClinicDto>>
{
    public int Id { get; set; }
}
