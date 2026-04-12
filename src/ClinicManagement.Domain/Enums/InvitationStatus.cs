namespace ClinicManagement.Domain.Enums;

/// <summary>Computed status of a staff invitation based on its flags and expiry.</summary>
public enum InvitationStatus
{
    Pending,
    Accepted,
    Canceled,
    Expired,
}
