using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IStaffInvitationRepository : IRepository<StaffInvitation>
{
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<StaffInvitation?> GetPendingInvitationByEmailAsync(string email, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffInvitation>> GetPendingInvitationsByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
}
