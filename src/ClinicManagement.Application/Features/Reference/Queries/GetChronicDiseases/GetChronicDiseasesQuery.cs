using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

public record GetChronicDiseasesQuery : IRequest<Result<List<ChronicDiseaseDto>>>;

public record ChronicDiseaseDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
}
