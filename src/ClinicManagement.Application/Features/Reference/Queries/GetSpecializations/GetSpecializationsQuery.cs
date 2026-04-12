using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public record GetSpecializationsQuery : IRequest<IEnumerable<SpecializationDto>>;

public record SpecializationDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);
