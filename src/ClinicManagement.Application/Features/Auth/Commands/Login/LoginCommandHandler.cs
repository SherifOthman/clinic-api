using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Utils;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;

using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        bool isEmail = StringUtils.IsEmail(request.Email);

        var user = isEmail
            ? await _identityService.GetUserByEmailAsync(request.Email, cancellationToken)
            : await _identityService.GetByUsernameAsync(request.Email, cancellationToken);

        if (user == null ||
            !await _identityService.CheckPasswordAsync(user, request.Password, cancellationToken))
            return Result<AuthResponseDto>.FailField("email", "Invalid username or password");

        var userRoles = await _identityService.GetUserRolesAsync(user, cancellationToken);
        
        // Get ClinicId based on user role
        int? clinicId = await GetUserClinicIdAsync(user.Id, userRoles, cancellationToken);
        
        var accessToken = _tokenService.GenerateAccessToken(user, userRoles, clinicId);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Result<AuthResponseDto>.Ok(response);
    }

    private async Task<int?> GetUserClinicIdAsync(int userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        // Check if user is a clinic owner
        if (roles.Contains("ClinicOwner"))
        {
            var clinic = await _unitOfWork.Clinics.GetByOwnerIdAsync(userId, cancellationToken);
            return clinic?.Id;
        }
        
        // Check if user is a doctor
        if (roles.Contains("Doctor"))
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId, cancellationToken);
            return doctor?.ClinicId;
        }
        
        // For other roles, no clinic association yet
        return null;
    }
}
