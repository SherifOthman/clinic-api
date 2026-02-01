using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Queries.GetClinic;

public record GetClinicQuery(Guid Id) : IRequest<Result<ClinicDto>>;
