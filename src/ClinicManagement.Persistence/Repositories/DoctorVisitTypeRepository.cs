using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class DoctorVisitTypeRepository : IDoctorVisitTypeRepository
{
    private readonly ApplicationDbContext _db;
    public DoctorVisitTypeRepository(ApplicationDbContext db) => _db = db;

    public Task<List<DoctorVisitType>> GetByDoctorAndBranchAsync(Guid doctorId, Guid branchId, CancellationToken ct = default)
        => _db.Set<DoctorVisitType>()
            .Where(v => v.DoctorId == doctorId && v.ClinicBranchId == branchId)
            .OrderBy(v => v.NameEn)
            .ToListAsync(ct);

    public Task<DoctorVisitType?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<DoctorVisitType>().FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<bool> HasAppointmentsAsync(Guid doctorVisitTypeId, CancellationToken ct = default)
        => _db.Set<Appointment>().AnyAsync(a => a.DoctorVisitTypeId == doctorVisitTypeId, ct);

    public void Add(DoctorVisitType visitType) => _db.Set<DoctorVisitType>().Add(visitType);
    public void Remove(DoctorVisitType visitType) => _db.Set<DoctorVisitType>().Remove(visitType);
}
