using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Abstractions.Services;

public interface IUserRegistrationService
{
    Task<int> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);
}
