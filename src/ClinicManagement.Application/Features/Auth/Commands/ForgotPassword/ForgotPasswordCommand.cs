using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    string Email
) : IRequest<ForgotPasswordResult>;
