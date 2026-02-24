using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffInvitationRepository : IRepository<StaffInvitation>
{
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffInvitation>> GetPendingByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
