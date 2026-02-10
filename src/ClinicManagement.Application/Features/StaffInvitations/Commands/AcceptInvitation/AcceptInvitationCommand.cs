using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;

public record AcceptInvitationCommand(AcceptInvitationDto Dto) : IRequest<Result>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRegistrationService _userRegistrationService;

    public AcceptInvitationCommandHandler(
        IApplicationDbContext context,
        IUserRegistrationService userRegistrationService)
    {
        _context = context;
        _userRegistrationService = userRegistrationService;
    }

    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Validate passwords match
        if (dto.Password != dto.ConfirmPassword)
        {
            return Result.FailField(nameof(dto.ConfirmPassword), MessageCodes.Validation.PASSWORDS_MUST_MATCH);
        }

        // Find invitation by token
        var invitation = await _context.StaffInvitations
            .FirstOrDefaultAsync(si => si.Token == dto.Token, cancellationToken);
        
        if (invitation == null)
        {
            return Result.Fail(MessageCodes.Invitation.INVALID_TOKEN);
        }

        // Check if already accepted
        if (invitation.IsAccepted)
        {
            return Result.Fail(MessageCodes.Invitation.ALREADY_ACCEPTED);
        }

        // Check if expired
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Fail(MessageCodes.Invitation.EXPIRED);
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
            if (registrationResult.Errors != null && registrationResult.Errors.Any())
                return Result.Fail(registrationResult.Errors);
            
            return Result.Fail(registrationResult.Code ?? MessageCodes.Authentication.REGISTRATION_FAILED);
        }

        // Mark invitation as accepted
        invitation.IsAccepted = true;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = registrationResult.Value;

        // Single SaveChanges - atomic transaction
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}