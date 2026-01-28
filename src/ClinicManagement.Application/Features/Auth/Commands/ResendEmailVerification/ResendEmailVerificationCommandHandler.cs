using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, Result>
{
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public ResendEmailVerificationCommandHandler(IUserManagementService userManagementService, IEmailConfirmationService emailConfirmationService)
    {
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            return Result.Fail(ApplicationErrors.Authentication.UserWithEmailNotFound(request.Email));

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
            return Result.Fail(ApplicationErrors.Authentication.EMAIL_ALREADY_CONFIRMED);

        var result = await _emailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);
        if (!result.Success)
            return Result.Fail($"Failed to send confirmation email: {result.Message}");

        return Result.Ok();
    }
}
