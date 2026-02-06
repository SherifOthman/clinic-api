using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.StaffInvitations.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserManagementService _userManagementService;

    public AcceptInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        IUserManagementService userManagementService)
    {
        _unitOfWork = unitOfWork;
        _userManagementService = userManagementService;
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
        var invitation = await _unitOfWork.StaffInvitations.GetByTokenAsync(dto.Token, cancellationToken);
        if (invitation == null)
        {
            return Result.Fail("INVITATION.INVALID_TOKEN");
        }

        // Check if already accepted
        if (invitation.IsAccepted)
        {
            return Result.Fail("INVITATION.ALREADY_ACCEPTED");
        }

        // Check if expired
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Fail("INVITATION.EXPIRED");
        }

        // Check if email already registered
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(invitation.Email);
        if (existingUser != null)
        {
            return Result.Fail(MessageCodes.Validation.EMAIL_ALREADY_REGISTERED);
        }

        // Start transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Generate GUID in code for transaction safety
            var userId = Guid.NewGuid();

            // Create user account
            var user = new User
            {
                Id = userId,
                UserName = invitation.Email,
                Email = invitation.Email,
                FirstName = invitation.FirstName,
                LastName = invitation.LastName,
                ClinicId = invitation.ClinicId,
                UserType = invitation.UserType,
                ProfileImageUrl = dto.ProfileImageUrl,
                EmailConfirmed = true, // Auto-confirm email for invited users
                OnboardingCompleted = true // Staff don't need onboarding
            };

            var createResult = await _userManagementService.CreateUserAsync(user, dto.Password, cancellationToken);
            if (createResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return createResult;
            }

            // Assign role based on UserType
            var roleName = invitation.UserType == UserType.Doctor ? Roles.Doctor : Roles.Receptionist;
            var roleResult = await _userManagementService.AddToRoleAsync(user, roleName, cancellationToken);
            if (roleResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return roleResult;
            }

            // Create type-specific entity
            if (invitation.UserType == UserType.Doctor)
            {
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                await _unitOfWork.Doctors.AddAsync(doctor);
            }
            else if (invitation.UserType == UserType.Receptionist)
            {
                var receptionist = new Receptionist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                await _unitOfWork.Receptionists.AddAsync(receptionist);
            }

            // Mark invitation as accepted
            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = userId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Ok();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
