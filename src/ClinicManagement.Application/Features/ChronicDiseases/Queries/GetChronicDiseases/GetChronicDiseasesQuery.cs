using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(
    string? Language = null
) : IRequest<Result<List<ChronicDiseaseDto>>>;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetChronicDiseasesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = await _context.ChronicDiseases
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        var chronicDiseaseDtos = chronicDiseases.Select(cd => new ChronicDiseaseDto
        {
            Id = cd.Id,
            NameEn = cd.NameEn,
            NameAr = cd.NameAr,
            DescriptionEn = cd.DescriptionEn,
            DescriptionAr = cd.DescriptionAr,
            // Set Name and Description based on language parameter or default to English
            Name = request.Language?.ToLower() == "ar" ? cd.NameAr : cd.NameEn,
            Description = request.Language?.ToLower() == "ar" ? cd.DescriptionAr : cd.DescriptionEn
        }).ToList();
        
        return Result<List<ChronicDiseaseDto>>.Ok(chronicDiseaseDtos);
    }
}
