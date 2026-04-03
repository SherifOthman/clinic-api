using ClinicManagement.Application.Abstractions.Data;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.Specializations.Queries;

public record GetAllSpecializationsQuery : IRequest<IEnumerable<SpecializationDto>>;

public record SpecializationDto(
    Guid Id,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr
);

public class GetAllSpecializationsHandler : IRequestHandler<GetAllSpecializationsQuery, IEnumerable<SpecializationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string CacheKey = "specializations";

    public GetAllSpecializationsHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<SpecializationDto>> Handle(GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var specializations = await _context.Specializations
                .ToListAsync(cancellationToken);
            return specializations.Adapt<IEnumerable<SpecializationDto>>();
        }) ?? Enumerable.Empty<SpecializationDto>();
    }
}
