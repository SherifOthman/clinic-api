using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Email;

public interface IEmailConfirmationService
{
    Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);
}
