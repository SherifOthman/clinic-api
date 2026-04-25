using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class StaffInvitation : AuditableTenantEntity, ISoftDeletable
{
    private const int DefaultExpirationDays = 7;

    public bool IsDeleted { get; set; } = false;

    public string Email { get; private set; } = null!;
    public ClinicMemberRole Role { get; private set; }
    public Guid? SpecializationId { get; private set; }
    public string InvitationToken { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsAccepted { get; private set; }
    public bool IsCanceled { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation properties
    public Specialization? Specialization { get; set; }
    public User? AcceptedByUser { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public bool IsExpired(DateTimeOffset currentTime) => currentTime >= ExpiresAt;
    public bool IsValid(DateTimeOffset currentTime) => !IsAccepted && !IsCanceled && !IsExpired(currentTime);
    public bool CanBeCanceled(DateTimeOffset currentTime) => !IsAccepted && !IsCanceled && !IsExpired(currentTime);

    public Result Accept(Guid userId, DateTimeOffset acceptedAt)
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
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_ACCEPTED, "Cannot cancel an accepted invitation");
        if (IsCanceled)
            return Result.Failure(ErrorCodes.INVITATION_ALREADY_CANCELED, "Invitation is already canceled");

        IsCanceled = true;
        return Result.Success();
    }

    public static StaffInvitation Create(
        Guid clinicId,
        string email,
        ClinicMemberRole role,
        Guid createdByUserId,
        Guid? specializationId = null,
        int expirationDays = DefaultExpirationDays)
    {
        return new StaffInvitation
        {
            ClinicId = clinicId,
            Email = email,
            Role = role,
            SpecializationId = specializationId,
            InvitationToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expirationDays),
            CreatedByUserId = createdByUserId,
        };
    }
}
