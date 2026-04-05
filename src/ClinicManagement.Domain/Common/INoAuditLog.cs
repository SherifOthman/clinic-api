namespace ClinicManagement.Domain.Common;

/// <summary>
/// Marker interface — entities implementing this extend AuditableEntity for
/// timestamp/soft-delete tracking, but do NOT generate standalone audit log rows.
/// Use for child/value entities that only make sense in the context of their parent
/// (e.g. PatientPhone, PatientChronicDisease, InvoiceItem, PrescriptionItem).
/// </summary>
public interface INoAuditLog { }
