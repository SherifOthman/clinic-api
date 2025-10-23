using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _identityService.ValidateUserAsync(request.Email, request.Password);
            if (!isValid)
                return Result<AuthResponseDto>.Failure("Invalid email or password");

            var user = await _identityService.GetUserByEmailAsync(request.Email);
            if (user == null)
                return Result<AuthResponseDto>.Failure("User not found");

            var accessToken = await _identityService.GenerateAccessTokenAsync(user);
            var refreshToken = await _identityService.GenerateRefreshTokenAsync(user);

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user)
            };

            return Result<AuthResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }
}
