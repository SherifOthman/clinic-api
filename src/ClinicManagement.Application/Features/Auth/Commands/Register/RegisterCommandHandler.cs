using AutoMapper;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExist  = await _identityService.GetUserByEmailAsync(request.Email);
        if (userExist != null)
            return Result.FailField("email",
                ErrorCodes.EmailAlreadyExists,
                "This email address is already registered.");

        userExist = await _identityService.GetByUsernameAsync(request.Username);
        if (userExist != null)
        {
            return Result.FailField("username",
                ErrorCodes.UsernameTaken,
                "This username is already taken.");
        }


        var user = _mapper.Map<User>(request);

        var result = await _identityService.CreateUserAsync(user, request.Password);

        if (!result.IsSuccess)
            return Result.Fail(result.Errors!);

        return Result.Ok();
    }


}
