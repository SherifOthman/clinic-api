using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Constants;

using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.SwitchClinic;

public class SwitchClinicCommandHandler : IRequestHandler<SwitchClinicCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public SwitchClinicCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponseDto>> Handle(SwitchClinicCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result<AuthResponseDto>.FailField("userId", "User not found");

        // Verify user belongs to the target clinic
        // This logic depends on how we store user-clinic relationships.
        // For now, let's assume we check if the user is the owner or a staff member of the clinic.
        
        var userRoles = await _identityService.GetUserRolesAsync(user, cancellationToken);
        bool hasAccess = false;

        if (userRoles.Contains("SystemAdmin"))
        {
            hasAccess = true;
        }
        else if (userRoles.Contains("ClinicOwner"))
        {
            var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.ClinicId, cancellationToken);
            if (clinic != null && clinic.OwnerId == user.Id)
            {
                hasAccess = true;
            }
            // TODO: Add support for multiple clinics ownership if needed
        }
        else
        {
            // Check if doctor/staff belongs to this clinic
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(user.Id, cancellationToken);
            if (doctor != null && doctor.ClinicId == request.ClinicId)
            {
                hasAccess = true;
            }
            // Add similar checks for other roles if needed
        }

        if (!hasAccess)
            return Result<AuthResponseDto>.FailField("clinicId", "User does not have access to this clinic");

        // Generate new tokens with the new ClinicId
        var accessToken = _tokenService.GenerateAccessToken(user, userRoles, request.ClinicId);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
}
