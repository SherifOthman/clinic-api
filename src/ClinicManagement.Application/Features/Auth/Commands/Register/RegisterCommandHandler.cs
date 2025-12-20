using AutoMapper;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;

using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailSender emailSender,
        IMapper mapper,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing registration for email: {Email}", request.Email);
        
        // Register user without clinic - they'll complete onboarding after email verification
        
        var userExist = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already registered", request.Email);
            return Result.FailField("email", "This email address is already registered.");
        }

        userExist = await _identityService.GetByUsernameAsync(request.Username, cancellationToken);
        if (userExist != null)
        {
            _logger.LogWarning("Registration failed: Username {Username} already taken", request.Username);
            return Result.FailField("username", "This username is already taken.");
        }

        var user = _mapper.Map<User>(request);
        user.ClinicId = null; // No clinic yet - will be set during onboarding

        var result = await _identityService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!result.IsSuccess)
        {
            var errors = result.Errors?.Select(e => e.Message).ToList() ?? new List<string> { "Registration failed" };
            _logger.LogError("Registration failed for email {Email}: {Errors}", request.Email, string.Join(", ", errors));
            return Result.Fail(result.Errors!);
        }

        await _identityService.SetUserRoleAsync(user, request.Role.ToString(), cancellationToken);
        
        // Try to send confirmation email, but don't fail registration if email fails
        try
        {
            await _identityService.SendConfirmationEmailAsync(user, cancellationToken);
            _logger.LogInformation("Registration successful for email: {Email}, UserId: {UserId}. Confirmation email sent.", request.Email, user.Id);
            return Result.Ok("Registration successful! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registration successful for email: {Email}, UserId: {UserId}, but failed to send confirmation email", request.Email, user.Id);
            return Result.Ok("Registration successful! However, we couldn't send the confirmation email. Please contact support to verify your account.");
        }
    }


}
