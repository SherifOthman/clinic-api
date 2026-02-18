

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordResult(
    bool Success
)
{
    public string? ErrorCode => null;
    public string? ErrorMessage => null;
};
