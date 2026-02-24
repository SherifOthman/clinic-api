using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Exceptions;

namespace ClinicManagement.Domain.Entities;

public class StaffInvitation : TenantEntity
{
    private const int DefaultExpirationDays = 7;

    public string Email { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public Guid? SpecializationId { get; private set; }
    public string InvitationToken { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsAccepted { get; private set; }
    public bool IsCanceled { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Business logic methods
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiresAt;
    
    public bool IsValid(DateTime currentTime) => !IsAccepted && !IsCanceled && !IsExpired(currentTime);
    
    public bool CanBeCanceled(DateTime currentTime) => !IsAccepted && !IsCanceled && !IsExpired(currentTime);

    public Result Accept(Guid userId, DateTime acceptedAt)
    {
        if (IsAccepted)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_ACCEPTED, "Invitation has already been accepted");
        
        if (IsCanceled)
            return Result.Failure(ErrorCodes.INVITATION_CANCELED, "Cannot accept a canceled invitation");
        
        if (IsExpired(acceptedAt))
            return Result.Failure(ErrorCodes.INVITATION_EXPIRED, "Cannot accept an expired invitation");

        IsAccepted = true;
        AcceptedAt = acceptedAt;
        AcceptedByUserId = userId;
        
        return Result.Success();
    }

    public Result Cancel()
    {
        if (IsAccepted)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_ACCEPTED, "Cannot cancel an invitation that has already been accepted");
        
        if (IsCanceled)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_CANCELED, "Invitation is already canceled");

        IsCanceled = true;
        return Result.Success();
    }

    // Factory method for creating new invitations
    public static StaffInvitation Create(
        Guid clinicId,
        string email,
        string role,
        Guid createdByUserId,
        Guid? specializationId = null,
        int expirationDays = DefaultExpirationDays)
    {
        var now = DateTime.UtcNow;
        
        return new StaffInvitation
        {
            ClinicId = clinicId,
            Email = email,
            Role = role,
            SpecializationId = specializationId,
            InvitationToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = now.AddDays(expirationDays),
            IsAccepted = false,
            IsCanceled = false,
            CreatedByUserId = createdByUserId,
            IsDeleted = false
        };
    }
}
