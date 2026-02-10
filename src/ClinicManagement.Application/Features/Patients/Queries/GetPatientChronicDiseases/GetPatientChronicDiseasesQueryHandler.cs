using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientChronicDiseases;

public record GetPatientChronicDiseasesQuery(Guid PatientId) : IRequest<Result<IEnumerable<PatientChronicDiseaseDto>>>;

public class GetPatientChronicDiseasesQueryHandler : IRequestHandler<GetPatientChronicDiseasesQuery, Result<IEnumerable<PatientChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPatientChronicDiseasesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PatientChronicDiseaseDto>>> Handle(GetPatientChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var allChronicDiseases = await _unitOfWork.Repository<PatientChronicDisease>()
            .GetAllAsync(cancellationToken);
        
        var patientDiseases = allChronicDiseases
            .Where(pcd => pcd.PatientId == request.PatientId)
            .OrderByDescending(pcd => pcd.CreatedAt);
        
        var dtos = patientDiseases.Adapt<IEnumerable<PatientChronicDiseaseDto>>();
        return Result<IEnumerable<PatientChronicDiseaseDto>>.Ok(dtos);
    }
}