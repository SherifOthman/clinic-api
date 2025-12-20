using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ForgotPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        
        // Don't reveal if user exists or not (security best practice)
        if (user == null)
        {
            return Result.Ok("If the email exists, a password reset link has been sent.");
        }

        await _identityService.SendPasswordResetEmailAsync(user, cancellationToken);

        return Result.Ok("If the email exists, a password reset link has been sent.");
    }
}
