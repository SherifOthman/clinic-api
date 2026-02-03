using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Queries.GetChronicDiseases;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<IEnumerable<ClinicPatientChronicDiseaseDto>>>
{
    private readonly IClinicPatientChronicDiseaseRepository _clinicPatientChronicDiseaseRepository;

    public GetChronicDiseasesQueryHandler(IClinicPatientChronicDiseaseRepository clinicPatientChronicDiseaseRepository)
    {
        _clinicPatientChronicDiseaseRepository = clinicPatientChronicDiseaseRepository;
    }

    public async Task<Result<IEnumerable<ClinicPatientChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var chronicDiseases = request.ActiveOnly
            ? await _clinicPatientChronicDiseaseRepository.GetActiveByClinicPatientIdAsync(request.ClinicPatientId, cancellationToken)
            : await _clinicPatientChronicDiseaseRepository.GetByClinicPatientIdAsync(request.ClinicPatientId, cancellationToken);

        var dtos = chronicDiseases.Adapt<IEnumerable<ClinicPatientChronicDiseaseDto>>();
        return Result<IEnumerable<ClinicPatientChronicDiseaseDto>>.Ok(dtos);
    }
}
