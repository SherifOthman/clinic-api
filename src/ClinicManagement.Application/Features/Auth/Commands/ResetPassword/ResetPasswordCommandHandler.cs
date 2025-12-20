using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Constants;

using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        
        if (user == null)
        {
            return Result.FailField("token", "Invalid or expired reset token");
        }

        var result = await _identityService.ResetPasswordAsync(user, request.Token, request.NewPassword, cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Ok(result.Message);
        }

        return Result.Fail(result.Message);
    }
}
