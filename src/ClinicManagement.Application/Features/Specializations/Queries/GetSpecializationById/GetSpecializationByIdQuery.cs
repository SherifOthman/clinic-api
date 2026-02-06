using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;

public record GetSpecializationByIdQuery(Guid Id) : IRequest<Result<SpecializationDto>>;
