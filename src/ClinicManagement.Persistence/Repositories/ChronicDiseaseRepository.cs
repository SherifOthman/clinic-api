using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class ChronicDiseaseRepository : Repository<ChronicDisease>, IChronicDiseaseRepository
{
    public ChronicDiseaseRepository(ApplicationDbContext context) : base(context) { }

    public Task<int> CountPatientsAsync(Guid chronicDiseaseId, CancellationToken ct = default)
        => Context.Set<PatientChronicDisease>()
            .CountAsync(p => p.ChronicDiseaseId == chronicDiseaseId, ct);
}
