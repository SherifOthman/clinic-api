using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record AcceptInvitationCommand(string Token, int UserId) : IRequest<AcceptInvitationResponse>;

public record AcceptInvitationResponse(bool Success, string Message, int? StaffId = null);

public class AcceptInvitationHandler : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AcceptInvitationResponse> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _unitOfWork.StaffInvitations.GetByTokenAsync(request.Token, cancellationToken);

        if (invitation == null)
            return new AcceptInvitationResponse(false, "Invitation not found");

        if (invitation.IsAccepted)
            return new AcceptInvitationResponse(false, "Invitation already accepted");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            return new AcceptInvitationResponse(false, "Invitation has expired");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            invitation.IsAccepted = true;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = request.UserId;

            await _unitOfWork.StaffInvitations.UpdateAsync(invitation, cancellationToken);

            var staff = new Domain.Entities.Staff
            {
                UserId = request.UserId,
                ClinicId = invitation.ClinicId,
                IsActive = true,
                HireDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Staff.AddAsync(staff, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new AcceptInvitationResponse(true, "Invitation accepted successfully", staff.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
