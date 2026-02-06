using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;

public record GetSpecializationsQuery : IRequest<Result<IEnumerable<SpecializationDto>>>;
