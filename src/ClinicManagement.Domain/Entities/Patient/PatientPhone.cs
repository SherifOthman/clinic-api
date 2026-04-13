using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone : BaseEntity
{
    public Guid PatientId { get; set; }

    /// <summary>Canonical E.164 format — e.g. +2001098021259</summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// National significant number — digits only, no country code, no trunk prefix.
    /// e.g. 1098021259 for Egypt (+20), 7911123456 for UK (+44).
    /// Stored separately so StartsWith search works for local-format input.
    /// </summary>
    public string NationalNumber { get; set; } = null!;
}
