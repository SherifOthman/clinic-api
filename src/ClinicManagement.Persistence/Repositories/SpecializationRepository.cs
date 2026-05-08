using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class SpecializationRepository : Repository<Specialization>, ISpecializationRepository
{
    public SpecializationRepository(ApplicationDbContext context) : base(context) { }

    public Task<int> CountDoctorsAsync(Guid specializationId, CancellationToken ct = default)
        => TenantGuard.AsUnfilteredQuery(Context.Set<DoctorInfo>())
            .CountAsync(d => d.SpecializationId == specializationId && !d.IsDeleted, ct);
}
