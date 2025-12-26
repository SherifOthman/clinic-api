using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands.AcceptInvitation;

public record AcceptInvitationCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public string Token { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}
