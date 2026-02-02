using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUserClinicRepository : IRepository<UserClinic>
{
    Task<List<UserClinic>> GetUserClinicsWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserClinic?> GetUserClinicWithDetailsAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default);
}