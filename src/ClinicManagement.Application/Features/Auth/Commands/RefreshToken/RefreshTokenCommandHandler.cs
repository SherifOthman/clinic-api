using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Options;
using ClinicManagement.Application.Common.Constants;

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
            return Result<AuthResponseDto>.FailField("refreshToken", "Invalid refresh token");

        var user = await _unitOfWork.Users.GetByIdAsync(oldToken.UserId);

        var userRoles = await _identityService.GetUserRolesAsync(user!, cancellationToken);
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        // Get ClinicId based on user role
        int? clinicId = await GetUserClinicIdAsync(user!.Id, userRoles, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user!, userRoles, clinicId);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user!, cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return response;
    }

    private async Task<int?> GetUserClinicIdAsync(int userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        // No clinic isolation in this simplified version
        return null;
    }
}
