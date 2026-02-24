using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Application.Abstractions.Services;

public interface IUserRegistrationService
{
    Task<Result<Guid>> RegisterUserAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);
}
