using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface ISpecializationRepository : IRepository<Specialization>
{
    /// <summary>Count of doctors assigned this specialization. Used to block hard-delete.</summary>
    Task<int> CountDoctorsAsync(Guid specializationId, CancellationToken ct = default);
}
