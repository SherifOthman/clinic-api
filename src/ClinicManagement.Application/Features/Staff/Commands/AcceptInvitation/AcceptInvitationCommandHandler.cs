using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Staff.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing invitation acceptance for user {UserId}", request.UserId);

        try
        {
            // 1. Get user
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Fail("Invalid invitation");
            }

            // 2. Verify user is not already confirmed
            if (user.EmailConfirmed)
            {
                return Result.Fail("This invitation has already been accepted");
            }

            // 3. Verify token (in production, store and validate actual tokens)
            // For now, we'll just check if the user exists and is inactive
            // TODO: Implement proper token storage and validation

            // 4. Check if username is available
            var existingUser = await _identityService.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return Result.FailField("username", "This username is already taken");
            }

            // 5. Update user details
            user.UserName = request.Username;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.PhoneNumber = request.PhoneNumber;
            }
            user.EmailConfirmed = true;

            // 6. Update password using the invitation token
            var passwordResult = await _identityService.ResetPasswordAsync(
                user, 
                request.Token,
                request.Password, 
                cancellationToken);

            if (!passwordResult.IsSuccess)
            {
                return Result.Fail("Failed to set password. " + passwordResult.Message);
            }

            // 7. Get user role to activate corresponding entity
            var roles = await _identityService.GetUserRolesAsync(user, cancellationToken);
            var role = roles.FirstOrDefault();

            if (role == UserRole.Doctor.ToString())
            {
                var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(user.Id, cancellationToken);
                if (doctor != null)
                {
                    doctor.IsActive = true;
                }
            }
            else if (role == UserRole.Receptionist.ToString())
            {
                // Activate receptionist if entity exists
                // var receptionist = await _unitOfWork.Receptionists.GetByUserIdAsync(user.Id, cancellationToken);
                // if (receptionist != null)
                // {
                //     receptionist.IsActive = true;
                // }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invitation accepted successfully for user {UserId}", request.UserId);

            return Result.Ok("Invitation accepted successfully! You can now log in.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation for user {UserId}", request.UserId);
            return Result.Fail("An error occurred while accepting the invitation. Please try again.");
        }
    }
}
