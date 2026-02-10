using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.InviteStaff;

public record InviteStaffCommand(InviteStaffDto Dto) : IRequest<Result<StaffInvitationDto>>;

public class InviteStaffCommandHandler : IRequestHandler<InviteStaffCommand, Result<StaffInvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;

    public InviteStaffCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
    }

    public async Task<Result<StaffInvitationDto>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Get current user
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<StaffInvitationDto>.FailSystem("UNAUTHENTICATED", "User is not authenticated");
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Result<StaffInvitationDto>.FailSystem("NOT_FOUND", "User not found");
        }

        // Verify user is a clinic owner
        if (user.UserType != UserType.ClinicOwner)
        {
            return Result<StaffInvitationDto>.FailBusiness("ACCESS_DENIED", "Only clinic owners can invite staff");
        }

        // Verify user has completed onboarding
        if (!user.OnboardingCompleted)
        {
            return Result<StaffInvitationDto>.FailBusiness("ONBOARDING_REQUIRED", "You must complete onboarding before inviting staff");
        }

        // Validate UserType (only Doctor or Receptionist)
        if (dto.UserType != UserType.Doctor && dto.UserType != UserType.Receptionist)
        {
            return Result<StaffInvitationDto>.FailValidation(nameof(dto.UserType), "User type must be Doctor or Receptionist");
        }

        // Check if email already exists
        var existingUser = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        
        if (existingUser != null)
        {
            return Result<StaffInvitationDto>.FailValidation(nameof(dto.Email), "Email is already registered");
        }

        // Ensure user has a clinic
        if (!user.ClinicId.HasValue)
        {
            return Result<StaffInvitationDto>.FailSystem("NO_CLINIC", "User does not have an associated clinic");
        }

        // Check if there's already a pending invitation for this email
        var existingInvitation = await _unitOfWork.Repository<StaffInvitation>()
            .FirstOrDefaultAsync(si => si.Email == dto.Email && si.ClinicId == user.ClinicId.Value && !si.IsAccepted && si.ExpiresAt > DateTime.UtcNow, cancellationToken);
        
        if (existingInvitation != null)
        {
            return Result<StaffInvitationDto>.FailValidation(nameof(dto.Email), "A pending invitation already exists for this email");
        }

        // Generate GUID in code for transaction safety
        var invitationId = Guid.NewGuid();
        var token = Guid.NewGuid().ToString("N"); // 32-character token without hyphens

        // Create invitation
        var invitation = new StaffInvitation
        {
            Id = invitationId,
            Email = dto.Email,
            UserType = dto.UserType,
            ClinicId = user.ClinicId.Value,
            InvitedByUserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
            IsAccepted = false
        };

        await _unitOfWork.Repository<StaffInvitation>().AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send invitation email
        var invitationLink = $"{GetBaseUrl()}/accept-invitation?token={token}";
        var emailBody = GetInvitationEmailTemplate(dto.Email, user.FirstName, user.LastName, invitationLink, dto.UserType);
        
        await _emailSender.SendEmailAsync(
            dto.Email,
            "You're Invited to Join Our Clinic",
            emailBody);

        // Map to DTO using Mapster
        var invitationDto = invitation.Adapt<StaffInvitationDto>();
        invitationDto.InvitedByUserName = $"{user.FirstName} {user.LastName}";

        return Result<StaffInvitationDto>.Ok(invitationDto);
    }

    private string GetBaseUrl()
    {
        // TODO: Move to configuration
        return "https://clinic.example.com";
    }

    private string GetInvitationEmailTemplate(string email, string inviterFirstName, string inviterLastName, string invitationLink, UserType userType)
    {
        var role = userType == UserType.Doctor ? "Doctor" : "Receptionist";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 6px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Clinic Invitation</h1>
        </div>
        <div class=""content"">
            <h2>Hello,</h2>
            <p>{inviterFirstName} {inviterLastName} has invited you to join their clinic as a <strong>{role}</strong>.</p>
            <p>Click the button below to accept the invitation and create your account:</p>
            <div style=""text-align: center;"">
                <a href=""{invitationLink}"" class=""button"">Accept Invitation</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #4F46E5;"">{invitationLink}</p>
            <p><strong>Note:</strong> This invitation will expire in 7 days.</p>
        </div>
        <div class=""footer"">
            <p>If you didn't expect this invitation, you can safely ignore this email.</p>
            <p>&copy; 2026 Clinic Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}