using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientByIdQueryHandler( IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetWithSurgeriesAsync(request.Id, cancellationToken);

            if (patient == null)
                return Result<PatientDto>.Failure("Patient not found");

            var patientDto = _mapper.Map<PatientDto>(patient);
            return Result<PatientDto>.Success(patientDto);
        }
        catch (Exception ex)
        {
            return Result<PatientDto>.Failure(ex.Message);
        }
    }
}
