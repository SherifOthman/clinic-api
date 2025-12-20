using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Clinics.Commands.UpdateClinic;

public class UpdateClinicCommandHandler : IRequestHandler<UpdateClinicCommand, Result<ClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateClinicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(UpdateClinicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.Id, cancellationToken);
            if (clinic == null)
                return Result<ClinicDto>.Fail("Clinic not found");

            clinic.Name = request.Name;
            clinic.Phone = request.Phone;
            clinic.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Clinics.Update(clinic);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return Result<ClinicDto>.Ok(clinicDto);
        }
        catch (Exception ex)
        {
            return Result<ClinicDto>.Fail(ex.Message);
        }
    }
}
