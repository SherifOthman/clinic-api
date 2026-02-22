using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffInvitationRepository : IRepository<StaffInvitation>
{
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffInvitation>> GetPendingByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingInvitationAsync(int clinicId, string role, CancellationToken cancellationToken = default);
    Task CancelInvitationAsync(int id, CancellationToken cancellationToken = default);
}
