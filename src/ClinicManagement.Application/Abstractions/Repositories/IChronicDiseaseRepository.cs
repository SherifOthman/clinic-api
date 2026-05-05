using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IChronicDiseaseRepository : IRepository<ChronicDisease>
{
    /// <summary>Count of patients who have this disease assigned. Used to block hard-delete.</summary>
    Task<int> CountPatientsAsync(Guid chronicDiseaseId, CancellationToken ct = default);
}
