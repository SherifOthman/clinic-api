using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IUserManagementService userManagementService,
        IEmailConfirmationService emailConfirmationService,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation attempt for non-existent user: {Email}", request.Email);
            return Result.Fail(ApplicationErrors.Authentication.USER_NOT_FOUND);
        }

        if (await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogInformation("Email confirmation attempt for already confirmed user: {Email}", request.Email);
            return Result.Ok();
        }
        
        var result = await _emailConfirmationService.ConfirmEmailAsync(user, request.Token, cancellationToken);
        if (result.Success)
        {
            _logger.LogInformation("Email confirmed successfully for user: {Email}", request.Email);
            return result;
        }

        _logger.LogWarning("Email confirmation failed for user: {Email}", request.Email);
        return Result.Fail(result.Code ?? "EMAIL.CONFIRMATION.FAILED");
    }
}
