using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Utils;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IIdentityService identityService,
        ITokenService tokenService,
        IMapper mapper)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        bool isEmail = CheckEmail.IsEmail(request.Email);

        var user = isEmail
            ? await _identityService.GetUserByEmailAsync(request.Email)
            : await _identityService.GetByUsernameAsync(request.Email);

        if (user == null ||
            !await _identityService.CheckPasswordAsync(user, request.Password))
            return Result<AuthResponseDto>.Failure("Invalid username or password");

        var userRoles = await _identityService.GetUserRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, userRoles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        };

        return Result<AuthResponseDto>.Success(response);
    }
}
