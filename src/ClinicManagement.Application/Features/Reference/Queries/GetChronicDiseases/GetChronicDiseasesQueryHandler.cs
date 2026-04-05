using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.Reference.Queries;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "ChronicDiseases";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public GetChronicDiseasesQueryHandler(
        IApplicationDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(
        GetChronicDiseasesQuery request,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out List<ChronicDiseaseDto>? cachedDiseases))
        {
            return Result<List<ChronicDiseaseDto>>.Success(cachedDiseases!);
        }

        var diseases = await _context.ChronicDiseases
            .OrderBy(d => d.NameEn)
            .ProjectToType<ChronicDiseaseDto>()
            .ToListAsync(cancellationToken);

        _cache.Set(CacheKey, diseases, CacheExpiration);

        return Result<List<ChronicDiseaseDto>>.Success(diseases);
    }
}
