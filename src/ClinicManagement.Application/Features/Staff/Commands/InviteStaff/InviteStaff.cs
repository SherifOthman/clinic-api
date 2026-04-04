using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record InviteStaffCommand(string Role, string Email, Guid? SpecializationId = null) : IRequest<Result<InviteStaffResponseDto>>;

public record InviteStaffResponseDto(Guid InvitationId, string Token, string ExpiresAt);

public class InviteStaffHandler : IRequestHandler<InviteStaffCommand, Result<InviteStaffResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IClock _clock;

    public InviteStaffHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IClock clock)
    {
        _context = context;
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
            var specializationExists = await _context.Specializations
                .AnyAsync(s => s.Id == request.SpecializationId.Value, cancellationToken);
                
            if (!specializationExists)
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

        _context.StaffInvitations.Add(invitation);

        var inviter = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        var invitedBy = inviter?.FullName ?? "Clinic Administrator";
        
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == clinicId, cancellationToken);
        var clinicName = clinic?.Name ?? "Clinic";

        var invitationLink = $"/accept-invitation/{invitation.InvitationToken}";

        await _emailService.SendStaffInvitationEmailAsync(
            request.Email,
            clinicName,
            request.Role,
            invitedBy,
            invitationLink,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new InviteStaffResponseDto(invitation.Id, invitation.InvitationToken, invitation.ExpiresAt.ToString("O"));
        return Result.Success(response);
    }
}
