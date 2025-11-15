using AutoMapper;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IEmailSender emailSender,
        IMapper mapper)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _mapper = mapper;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExist = await _identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userExist != null)
            return Result.FailField("email", "This email address is already registered.");

        userExist = await _identityService.GetByUsernameAsync(request.Username, cancellationToken);
        if (userExist != null)
        {
            return Result.FailField("username", "This username is already taken.");
        }

        var user = _mapper.Map<User>(request);

        var result = await _identityService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!result.IsSuccess)
            return Result.Fail(result.Errors!);

        await _identityService.SetUserRoleAsync(user, request.Role.ToString(), cancellationToken);
        await _identityService.SendConfirmationEmailAsync(user, cancellationToken);

        return Result.Ok();
    }


}
