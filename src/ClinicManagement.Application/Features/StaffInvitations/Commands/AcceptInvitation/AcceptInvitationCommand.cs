using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;

public record AcceptInvitationCommand(AcceptInvitationDto Dto) : IRequest<Result>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRegistrationService _userRegistrationService;

    public AcceptInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRegistrationService userRegistrationService)
    {
        _unitOfWork = unitOfWork;
        _userRegistrationService = userRegistrationService;
    }

    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Validate passwords match
        if (dto.Password != dto.ConfirmPassword)
        {
            return Result.FailValidation(nameof(dto.ConfirmPassword), "Passwords must match");
        }

        // Find invitation by token
        var invitation = await _unitOfWork.Repository<StaffInvitation>()
            .FirstOrDefaultAsync(si => si.Token == dto.Token, cancellationToken);
        
        if (invitation == null)
        {
            return Result.FailBusiness("INVALID_TOKEN", "Invalid invitation token");
        }

        // Check if already accepted
        if (invitation.IsAccepted)
        {
            return Result.FailBusiness("ALREADY_ACCEPTED", "This invitation has already been accepted");
        }

        // Check if expired
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            return Result.FailBusiness("INVITATION_EXPIRED", "This invitation has expired");
        }

        // Register user using the shared service
        var registrationRequest = dto.Adapt<UserRegistrationRequest>();
        registrationRequest.Email = invitation.Email;
        registrationRequest.UserName = dto.UserName ?? invitation.Email;
        registrationRequest.UserType = invitation.UserType;
        registrationRequest.ClinicId = invitation.ClinicId;
        registrationRequest.EmailConfirmed = true; // Auto-confirm email for invited users
        registrationRequest.OnboardingCompleted = true; // Staff don't need onboarding
        registrationRequest.SendConfirmationEmail = false; // Don't send confirmation email

        var registrationResult = await _userRegistrationService.RegisterUserAsync(registrationRequest, cancellationToken);
        if (registrationResult.IsFailure)
        {
            if (registrationResult.ValidationErrors != null && registrationResult.ValidationErrors.Any())
                return Result.FailValidation(registrationResult.ValidationErrors);
            
            return Result.FailSystem("REGISTRATION_FAILED", registrationResult.Message ?? "User registration failed");
        }

        // Mark invitation as accepted
        invitation.IsAccepted = true;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = registrationResult.Value;

        // Single SaveChanges - atomic transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}