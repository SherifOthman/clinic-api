using MediatR;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;

public record GetSpecializationsQuery : IRequest<List<SpecializationDto>>;
