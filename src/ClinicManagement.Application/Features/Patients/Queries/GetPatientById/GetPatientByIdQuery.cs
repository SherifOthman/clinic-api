using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<Result<PatientDto>>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId!.Value;

        var patient = await _unitOfWork.Patients.GetByIdForClinicAsync(request.Id, clinicId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.FailSystem("NOT_FOUND", "Patient not found");
        }

        // Load with includes for DTO mapping
        patient = await _unitOfWork.Patients.GetByIdWithIncludesAsync(request.Id, cancellationToken);

        var patientDto = patient!.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
