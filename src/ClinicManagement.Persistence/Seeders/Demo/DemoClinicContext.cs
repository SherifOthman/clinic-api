namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Carries resolved IDs from the demo clinic setup to downstream seeders.
/// Avoids repeated DB lookups across seeder classes.
/// </summary>
public class DemoClinicContext
{
    public Guid ClinicId       { get; init; }
    public Guid BranchId       { get; init; }
    public Guid OwnerUserId    { get; init; }

    // ── Doctor 1: Time-based (the original demo doctor) ──────────────────────
    public Guid DoctorUserId   { get; init; }
    public Guid DoctorInfoId   { get; init; }
    public Guid VisitTypeId    { get; init; }   // Consultation 150
    public Guid VisitType2Id   { get; init; }   // Follow-up 80

    // ── Doctor 2: Queue-based ─────────────────────────────────────────────────
    public Guid Doctor2InfoId  { get; init; }
    public Guid VisitType3Id   { get; init; }   // General Checkup 100
    public Guid VisitType4Id   { get; init; }   // Vaccination 50

    // ── Doctor 3: Time-based (different specialization) ───────────────────────
    public Guid Doctor3InfoId  { get; init; }
    public Guid VisitType5Id   { get; init; }   // Cardiology Consult 200
    public Guid VisitType6Id   { get; init; }   // ECG 120
}
