using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(IIdentityService identityService, IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = new User
            {
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                ThirdName = request.ThirdName,
                Email = request.Email,
                Avatar = request.Avatar,
                PhoneNumber = request.PhoneNumber
            };

            var userId = await _identityService.CreateUserAsync(user, request.Password);
            var createdUser = await _identityService.GetUserByIdAsync(userId);
            
            if (createdUser == null)
                return Result<AuthResponseDto>.Failure("Failed to create user");

            var accessToken = await _identityService.GenerateAccessTokenAsync(createdUser);
            var refreshToken = await _identityService.GenerateRefreshTokenAsync(createdUser);

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(createdUser)
            };

            return Result<AuthResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }
}
