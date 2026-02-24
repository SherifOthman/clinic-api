using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record InviteStaffCommand(string Role, string Email, Guid? SpecializationId = null) : IRequest<Result<InviteStaffResponseDto>>;

public record InviteStaffResponseDto(Guid InvitationId, string Token, DateTime ExpiresAt);

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IClock _clock;
    

    public InviteStaffHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IClock clock
        )
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _clock = clock;
    }

    public async Task<Result<InviteStaffResponseDto>> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();
        var clinicId = _currentUserService.GetRequiredClinicId();

        if (request.Role != "Doctor" && request.Role != "Receptionist")
            return Result.Failure<InviteStaffResponseDto>(ErrorCodes.VALIDATION_ERROR, "Role must be either Doctor or Receptionist");

        if (request.Role == "Doctor" && request.SpecializationId.HasValue)
        {
            var specialization = await _unitOfWork.Specializations.GetByIdAsync(request.SpecializationId.Value, cancellationToken);
            if (specialization == null)
                return Result.Failure<InviteStaffResponseDto>(ErrorCodes.NOT_FOUND, "Specialization not found");
        }

        var invitation = StaffInvitation.Create(
            clinicId,
            request.Email,
            request.Role,
            currentUserId,
            _clock.UtcNow,
            request.SpecializationId
        );

        await _unitOfWork.StaffInvitations.AddAsync(invitation, cancellationToken);

        var inviter = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);
        var invitedBy = inviter?.FullName ?? "Clinic Administrator";
        
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId, cancellationToken);
        var clinicName = clinic?.Name ?? "Clinic";

        var invitationLink = $"/accept-invitation/{invitation.InvitationToken}";

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinicName,
            request.Role,
            invitedBy,
            invitationLink,
            cancellationToken);

        var response = new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt);
        return Result.Success(response);
    }
}
