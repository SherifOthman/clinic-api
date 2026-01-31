using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IClinicManagementService
{
    Task<Result<bool>> AssignUserToClinicAsync(int userId, int clinicId, bool isOwner = false, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveUserFromClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Clinic>>> GetUserClinicsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CanUserAccessClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsUserClinicOwnerAsync(int userId, int clinicId, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUserClinicCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CanUserCreateMoreClinicsAsync(int userId, CancellationToken cancellationToken = default);
}