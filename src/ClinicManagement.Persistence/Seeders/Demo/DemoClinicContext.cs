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
    public Guid DoctorUserId   { get; init; }
    public Guid DoctorInfoId   { get; init; }
    public Guid VisitTypeId    { get; init; }
    public Guid VisitType2Id   { get; init; }
}
