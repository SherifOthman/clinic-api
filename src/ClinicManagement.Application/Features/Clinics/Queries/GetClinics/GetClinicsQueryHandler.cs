using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinics;

public class GetClinicsQueryHandler : IRequestHandler<GetClinicsQuery, Result<List<ClinicDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClinicsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ClinicDto>>> Handle(GetClinicsQuery request, CancellationToken cancellationToken)
    {
        var clinics = await _unitOfWork.Clinics.GetClinicsPagedAsync(
            request.OwnerId, 
            request.IsActive, 
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        var clinicDtos = _mapper.Map<List<ClinicDto>>(clinics);
        return Result<List<ClinicDto>>.Ok(clinicDtos);
    }
}
