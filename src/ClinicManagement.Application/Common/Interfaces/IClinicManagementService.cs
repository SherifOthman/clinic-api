using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IClinicManagementService
{
    Task<Result<bool>> AssignUserToClinicAsync(Guid userId, Guid clinicId, bool isOwner = false, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveUserFromClinicAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Clinic>>> GetUserClinicsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CanUserAccessClinicAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsUserClinicOwnerAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUserClinicCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CanUserCreateMoreClinicsAsync(Guid userId, CancellationToken cancellationToken = default);
}