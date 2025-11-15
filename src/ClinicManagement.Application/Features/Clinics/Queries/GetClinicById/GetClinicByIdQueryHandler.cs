using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Queries.GetClinicById;

public class GetClinicByIdQueryHandler : IRequestHandler<GetClinicByIdQuery, Result<ClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClinicByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(GetClinicByIdQuery request, CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.Id, cancellationToken);

        if (clinic == null)
            return Result<ClinicDto>.Fail("Clinic not found");

        var clinicDto = _mapper.Map<ClinicDto>(clinic);
        return Result<ClinicDto>.Ok(clinicDto);
    }
}
