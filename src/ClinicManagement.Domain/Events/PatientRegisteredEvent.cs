using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Events;

/// <summary>
/// Domain event raised when a new patient is registered in the system
/// This event can trigger side effects like:
/// - Sending welcome SMS/email
/// - Creating initial medical file
/// - Logging for analytics
/// - Notifying relevant staff
/// </summary>
public sealed record PatientRegisteredEvent : DomainEvent
{
    public Guid PatientId { get; init; }
    public Guid ClinicId { get; init; }
    public string PatientCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string PrimaryPhoneNumber { get; init; } = null!;
    public DateTime DateOfBirth { get; init; }

    public PatientRegisteredEvent(
        Guid patientId,
        Guid clinicId,
        string patientCode,
        string fullName,
        string primaryPhoneNumber,
        DateTime dateOfBirth)
    {
        PatientId = patientId;
        ClinicId = clinicId;
        PatientCode = patientCode;
        FullName = fullName;
        PrimaryPhoneNumber = primaryPhoneNumber;
        DateOfBirth = dateOfBirth;
    }
}
