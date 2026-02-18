using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IUserRegistrationService
{
    Task<Guid> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);
}
