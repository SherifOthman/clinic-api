using AutoMapper;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedList<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPatientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await _unitOfWork.Patients.GetPatientsPagedAsync(
            request.ClinicId,
            request.SearchTerm,
            request.Gender,
            request.City,
            request.MinAge,
            request.MaxAge,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var patientDtos = _mapper.Map<List<PatientDto>>(patients);
        
        var result = new PaginatedList<PatientDto>(
            patientDtos,
            patientDtos.Count,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<PatientDto>>.Ok(result);
    }
}
