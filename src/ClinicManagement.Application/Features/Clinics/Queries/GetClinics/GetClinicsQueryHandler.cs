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
        try
        {
            IEnumerable<Clinic> clinics;

            if (request.OwnerId.HasValue)
            {
                clinics = await _unitOfWork.Clinics.GetByOwnerIdAsync(request.OwnerId.Value, cancellationToken);
            }
            else if (request.IsActive.HasValue)
            {
                clinics = await _unitOfWork.Clinics.GetActiveClinicsAsync(cancellationToken);
            }
            else
            {
                clinics = await _unitOfWork.Clinics.GetAllAsync(cancellationToken);
            }

            var pagedClinics = clinics
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var clinicDtos = _mapper.Map<List<ClinicDto>>(pagedClinics);
            return Result<List<ClinicDto>>.Ok(clinicDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicDto>>.Fail(ex.Message);
        }
    }
}
