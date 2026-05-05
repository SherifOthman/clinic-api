using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

/// <summary>Admin-only paginated query — includes inactive, no cache.</summary>
public record GetChronicDiseasesPaginatedQuery(
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<ChronicDiseaseAdminDto>>>;

public record ChronicDiseaseAdminDto(
    Guid    Id,
    string  NameEn,
    string  NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool    IsActive
);

public class GetChronicDiseasesPaginatedHandler
    : IRequestHandler<GetChronicDiseasesPaginatedQuery, Result<PaginatedResult<ChronicDiseaseAdminDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetChronicDiseasesPaginatedHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<ChronicDiseaseAdminDto>>> Handle(
        GetChronicDiseasesPaginatedQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.Reference.GetChronicDiseasesPaginatedAsync(
            request.PageNumber, request.PageSize, ct);

        var dtos = items.Select(d => new ChronicDiseaseAdminDto(
            d.Id, d.NameEn, d.NameAr, d.DescriptionEn, d.DescriptionAr, d.IsActive)).ToList();

        return Result.Success(
            PaginatedResult<ChronicDiseaseAdminDto>.Create(dtos, total, request.PageNumber, request.PageSize));
    }
}
