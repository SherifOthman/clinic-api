using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IReceptionistRepository : IRepository<Receptionist>
{
    Task<Receptionist?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Receptionist>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
}