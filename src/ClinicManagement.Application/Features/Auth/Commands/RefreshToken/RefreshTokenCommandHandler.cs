using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _identityService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!isValid)
                return Result<AuthResponseDto>.Failure("Invalid refresh token");

            // Get user from refresh token (implementation depends on your token service)
            // For now, we'll need to modify the IIdentityService to return user from refresh token
            var user = await _identityService.GetUserByEmailAsync(""); // This needs to be implemented properly
            
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
