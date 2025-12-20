using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _identityService = identityService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
            return Result<UserDto>.Fail("User not found");

        var userDto = _mapper.Map<UserDto>(user);
        
        // Get roles from Identity's AspNetUserRoles table
        var roles = await _identityService.GetUserRolesAsync(user, cancellationToken);
        userDto.Roles = roles.ToList();
        userDto.Role = roles.FirstOrDefault();
        userDto.FullName = $"{user.FirstName} {user.LastName}";

        return Result<UserDto>.Ok(userDto);
    }
}
