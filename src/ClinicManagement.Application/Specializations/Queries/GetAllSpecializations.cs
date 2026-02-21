using ClinicManagement.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Specializations.Queries;

public record GetAllSpecializationsQuery : IRequest<IEnumerable<SpecializationDto>>;

public record SpecializationDto(
    int Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);

public class GetAllSpecializationsHandler : IRequestHandler<GetAllSpecializationsQuery, IEnumerable<SpecializationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string CacheKey = "specializations";

    public GetAllSpecializationsHandler(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IEnumerable<SpecializationDto>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var specializations = await _unitOfWork.Specializations.GetAllAsync(cancellationToken);
            return specializations.Adapt<IEnumerable<SpecializationDto>>();
        }) ?? Enumerable.Empty<SpecializationDto>();
    }
}
