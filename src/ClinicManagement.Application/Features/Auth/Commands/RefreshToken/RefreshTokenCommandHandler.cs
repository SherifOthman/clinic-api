using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(IIdentityService identityService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var oldToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (oldToken == null || !oldToken.IsActive)
            return Result<AuthResponseDto>.Fail("Invalid refresh token");

        var user = await _unitOfWork.Users.GetByIdAsync(oldToken.UserId);

        var userRoles = await _identityService.GetUserRolesAsync(user!);
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);


        var accessToken = _tokenService.GenerateAccessToken(user!, userRoles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user!);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user)
        };

        return response;
    }
}
