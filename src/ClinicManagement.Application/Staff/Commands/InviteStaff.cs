using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record InviteStaffCommand(string Role, string Email) : IRequest<Result<InviteStaffResponse>>;

public record InviteStaffResponse(int InvitationId, string Token, DateTime ExpiresAt);

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    

    public InviteStaffHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService
        )
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<Result<InviteStaffResponse>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var clinicId = _currentUserService.ClinicId;

        if (!currentUserId.HasValue || !clinicId.HasValue)
            return Result.Failure<InviteStaffResponse>(ErrorCodes.UNAUTHORIZED, "User must be authenticated and belong to a clinic");

        if (request.Role != "Doctor" && request.Role != "Receptionist")
            return Result.Failure<InviteStaffResponse>(ErrorCodes.VALIDATION_ERROR, "Role must be either Doctor or Receptionist");

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var invitation = new StaffInvitation
        {
            ClinicId = clinicId.Value,
            Email = request.Email,
            Role = request.Role,
            InvitationToken = token,
            ExpiresAt = expiresAt,
            IsAccepted = false,
            CreatedByUserId = currentUserId.Value,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.StaffInvitations.AddAsync(invitation, cancellationToken);

        // Get inviter information for email
        var inviter = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value, cancellationToken);
        var invitedBy = inviter?.FullName ?? "Clinic Administrator";
        
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
        var clinicName = clinic?.Name ?? "Clinic";

        // Build invitation link - will be constructed in EmailService with FrontendUrl from config
        var invitationLink = $"/accept-invitation/{token}";

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinicName,
            request.Role,
            invitedBy,
            invitationLink,
            cancellationToken);

        var response = new InviteStaffResponse(invitation.Id, token, expiresAt);
        return Result.Success(response);
    }
}
