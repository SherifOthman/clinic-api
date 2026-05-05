using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Reference.Queries;

/// <summary>Admin-only paginated query — includes inactive, no cache.</summary>
public record GetSpecializationsPaginatedQuery(
    int PageNumber = 1,
    int PageSize   = 10
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<SpecializationAdminDto>>>;

public record SpecializationAdminDto(
    Guid    Id,
    string  NameEn,
    string  NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    bool    IsActive
);

public class GetSpecializationsPaginatedHandler
    : IRequestHandler<GetSpecializationsPaginatedQuery, Result<PaginatedResult<SpecializationAdminDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetSpecializationsPaginatedHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<SpecializationAdminDto>>> Handle(
        GetSpecializationsPaginatedQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.Reference.GetSpecializationsPaginatedAsync(
            request.PageNumber, request.PageSize, ct);

        var dtos = items.Select(s => new SpecializationAdminDto(
            s.Id, s.NameEn, s.NameAr, s.DescriptionEn, s.DescriptionAr, s.IsActive)).ToList();

        return Result.Success(
            PaginatedResult<SpecializationAdminDto>.Create(dtos, total, request.PageNumber, request.PageSize));
    }
}
