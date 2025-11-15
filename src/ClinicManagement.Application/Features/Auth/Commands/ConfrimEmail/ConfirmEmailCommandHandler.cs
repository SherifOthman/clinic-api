using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.ConfrimEmail;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfrimEmailCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public ConfirmEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IIdentityService identityService
    )
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result> Handle(ConfrimEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Fail($"User with {request.UserId} was not found!");

        var result = await _identityService.ConfirmEmailAsync(user, request.Token, cancellationToken);

        if (result.IsSuccess)
            return Result.Ok(result.Message);

        return Result.Fail(result.Message);
    }
}
