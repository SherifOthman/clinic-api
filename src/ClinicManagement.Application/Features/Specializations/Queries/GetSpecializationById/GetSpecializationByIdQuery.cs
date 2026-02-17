using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;
using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;

public record GetSpecializationByIdQuery(Guid Id) : IRequest<SpecializationDto?>;
