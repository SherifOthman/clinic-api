using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandHandler : IRequestHandler<CompleteOnboardingCommand, Result<ClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CompleteOnboardingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClinicDto>> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        // Check if user already has a clinic
        var existingClinic = await _unitOfWork.Clinics.GetByOwnerIdAsync(userId, cancellationToken);
        if (existingClinic != null)
        {
            return Result<ClinicDto>.Fail("User already has a clinic associated with their account.");
        }

        // Create new clinic using Mapster
        var clinic = request.Adapt<Clinic>();
        clinic.OwnerId = userId; // Set from current user context

        _unitOfWork.Clinics.Add(clinic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update user's clinic association
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.ClinicId = clinic.Id;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Return clinic DTO using Mapster
        var clinicDto = clinic.Adapt<ClinicDto>();

        return Result<ClinicDto>.Ok(clinicDto);
    }
}