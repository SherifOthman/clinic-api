using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Abstractions.Services;

public interface IUserRegistrationService
{
    Task<Guid> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);
}
