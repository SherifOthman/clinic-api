using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinicById;

public class GetClinicByIdQueryHandler : IRequestHandler<GetClinicByIdQuery, Result<ClinicDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetClinicByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(GetClinicByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _context.UnitOfWork.Clinics.GetByIdAsync(request.Id, cancellationToken);

            if (clinic == null)
                return Result<ClinicDto>.Failure("Clinic not found");

            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return Result<ClinicDto>.Success(clinicDto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Failure(ex.Message);
        }
    }
}
