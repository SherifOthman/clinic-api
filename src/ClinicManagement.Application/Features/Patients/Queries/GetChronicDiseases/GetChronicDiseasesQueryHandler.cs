using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetChronicDiseases;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<IEnumerable<PatientChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChronicDiseasesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PatientChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = request.ActiveOnly
            ? await _unitOfWork.PatientChronicDiseases.GetActiveByPatientIdAsync(request.PatientId, cancellationToken)
            : await _unitOfWork.PatientChronicDiseases.GetByPatientIdAsync(request.PatientId, cancellationToken);

        var dtos = chronicDiseases.Adapt<IEnumerable<PatientChronicDiseaseDto>>();
        return Result<IEnumerable<PatientChronicDiseaseDto>>.Ok(dtos);
    }
}
