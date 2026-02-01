using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<List<ChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChronicDiseasesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = await _unitOfWork.ChronicDiseases.GetActiveAsync(cancellationToken);
        var chronicDiseaseDtos = chronicDiseases.Select(cd => new ChronicDiseaseDto
        {
            Id = cd.Id,
            NameEn = cd.NameEn,
            NameAr = cd.NameAr,
            DescriptionEn = cd.DescriptionEn,
            DescriptionAr = cd.DescriptionAr,
            // Set Name and Description based on request language or default to English
            Name = request.Language?.ToLower() == "ar" ? cd.NameAr : cd.NameEn,
            Description = request.Language?.ToLower() == "ar" ? cd.DescriptionAr : cd.DescriptionEn,
            IsActive = cd.IsActive
        }).ToList();
        
        return Result<List<ChronicDiseaseDto>>.Ok(chronicDiseaseDtos);
    }
}
