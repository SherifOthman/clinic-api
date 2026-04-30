using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public record GetSpecializationsQuery : IRequest<Result<List<SpecializationDto>>>;

public record SpecializationDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);
