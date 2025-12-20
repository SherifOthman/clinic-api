using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Constants;

using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public ChangePasswordCommandHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        if (!userId.HasValue)
        {
            return Result.FailField("", "User not authenticated");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
        
        if (user == null)
        {
            return Result.FailField("", "User not found");
        }

        var result = await _identityService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword, cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Ok(result.Message);
        }

        return Result.Fail(result.Message);
    }
}
