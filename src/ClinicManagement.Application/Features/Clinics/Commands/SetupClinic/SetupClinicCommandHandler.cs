using AutoMapper;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Commands.SetupClinic;

public class SetupClinicCommandHandler : IRequestHandler<SetupClinicCommand, Result<ClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SetupClinicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ClinicDto>> Handle(SetupClinicCommand request, CancellationToken cancellationToken)
    {
        // Check if user already has a clinic
        var existingClinics = await _unitOfWork.Clinics.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        if (existingClinics.Any())
        {
            return Result<ClinicDto>.Fail("User already has a clinic");
        }

        var clinic = new Clinic
        {
            Name = request.Name,
            Phone = request.Phone,
            OwnerId = request.OwnerId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };

        _unitOfWork.Clinics.Add(clinic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var clinicDto = _mapper.Map<ClinicDto>(clinic);
        return Result<ClinicDto>.Ok(clinicDto);
    }
}
