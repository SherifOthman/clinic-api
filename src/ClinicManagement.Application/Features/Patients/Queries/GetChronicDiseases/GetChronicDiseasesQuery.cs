using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(Guid PatientId) : IRequest<Result<IEnumerable<PatientChronicDiseaseDto>>>;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<IEnumerable<PatientChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChronicDiseasesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PatientChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = await _unitOfWork.Repository<PatientChronicDisease>()
            .GetAllAsync(pcd => pcd.PatientId == request.PatientId, cancellationToken);

        // Note: IsActive filter would require additional properties on the entity
        // Currently PatientChronicDisease is a simple junction table
        
        var dtos = chronicDiseases.Adapt<IEnumerable<PatientChronicDiseaseDto>>();
        
        return Result<IEnumerable<PatientChronicDiseaseDto>>.Ok(dtos);
    }
}