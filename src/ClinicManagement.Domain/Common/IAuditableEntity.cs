namespace ClinicManagement.Domain.Common;

/// <summary>
/// Opt-in marker interface for entities whose data changes should be
/// automatically captured in the audit log via the EF SaveChanges interceptor.
///
/// Only add this to entities where field-level diffs are meaningful to admins.
/// Do NOT add to: reference tables, junction tables, AuditLog itself,
/// or entities whose changes are better captured as business events (IAuditableCommand).
///
/// Entities that implement this:
///   Patient, Appointment, ClinicMember, DoctorInfo, ClinicBranch, Clinic
/// </summary>
public interface IAuditableEntity { }
