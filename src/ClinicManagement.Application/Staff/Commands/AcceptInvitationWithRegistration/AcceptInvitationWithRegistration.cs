using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record AcceptInvitationWithRegistrationCommand(
    string Token,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
) : IRequest<Result>;

public class AcceptInvitationWithRegistrationHandler : IRequestHandler<AcceptInvitationWithRegistrationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public AcceptInvitationWithRegistrationHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(AcceptInvitationWithRegistrationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _unitOfWork.StaffInvitations.GetByTokenAsync(request.Token, cancellationToken);

        if (invitation == null)
            return Result.Failure("InvitationNotFound", "Invitation not found");

        if (invitation.IsAccepted)
            return Result.Failure("InvitationAlreadyAccepted", "Invitation already accepted");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            return Result.Failure("InvitationExpired", "Invitation has expired");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create user account
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Email = invitation.Email, // Use email from invitation to ensure it matches
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                IsEmailConfirmed = true
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            // Get role by name and assign it
            var roles = await _unitOfWork.Users.GetRolesAsync(cancellationToken);
            var role = roles.FirstOrDefault(r => r.Name == invitation.Role);
            
            if (role != null)
            {
                await _unitOfWork.Users.AddUserRoleAsync(user.Id, role.Id, cancellationToken);
            }

            // Update invitation
            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = user.Id;

            await _unitOfWork.StaffInvitations.UpdateAsync(invitation, cancellationToken);

            // Create staff record
            var staff = new Domain.Entities.Staff
            {
                UserId = user.Id,
                ClinicId = invitation.ClinicId,
                IsActive = true,
                HireDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Staff.AddAsync(staff, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
