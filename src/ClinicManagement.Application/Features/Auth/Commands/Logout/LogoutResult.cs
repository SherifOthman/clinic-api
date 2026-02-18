

namespace ClinicManagement.Application.Features.Auth.Commands.Logout;

public record LogoutResult(
    bool Success
)
{
    public string? ErrorCode => null;
    public string? ErrorMessage => null;
};
