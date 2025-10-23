using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinics;

public class GetClinicsQueryHandler : IRequestHandler<GetClinicsQuery, Result<List<ClinicDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetClinicsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<ClinicDto>>> Handle(GetClinicsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Clinic> clinics;

            if (request.OwnerId.HasValue)
            {
                clinics = await _context.UnitOfWork.Clinics.GetByOwnerIdAsync(request.OwnerId.Value, cancellationToken);
            }
            else if (request.IsActive.HasValue)
            {
                clinics = await _context.UnitOfWork.Clinics.GetActiveClinicsAsync(cancellationToken);
            }
            else
            {
                clinics = await _context.UnitOfWork.Clinics.GetAllAsync(cancellationToken);
            }

            var pagedClinics = clinics
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var clinicDtos = _mapper.Map<List<ClinicDto>>(pagedClinics);
            return Result<List<ClinicDto>>.Success(clinicDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClinicDto>>.Failure(ex.Message);
        }
    }
}
