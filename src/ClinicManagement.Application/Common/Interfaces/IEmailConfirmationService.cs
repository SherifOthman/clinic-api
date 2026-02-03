using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IEmailConfirmationService
{
    Task<Result> SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);
    Task<string> GenerateEmailConfirmationTokenAsync(User user, CancellationToken cancellationToken = default);
}
