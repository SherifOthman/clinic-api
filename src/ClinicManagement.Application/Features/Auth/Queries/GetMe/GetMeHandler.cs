using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class GetMeHandler : IRequestHandler<GetMeQuery, Result<GetMeDto>>
{
    private readonly IUserRepository       _users;
    private readonly IPermissionRepository _permissions;

    public GetMeHandler(IUserRepository users, IPermissionRepository permissions)
    {
        _users       = users;
        _permissions = permissions;
    }

    public async Task<Result<GetMeDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var projection = await _users.GetMeProjectionAsync(request.UserId, cancellationToken);
        if (projection is null)
            return Result.Failure<GetMeDto>(ErrorCodes.NOT_FOUND, "User not found");

        var roles       = await _users.GetRolesByUserIdAsync(request.UserId, cancellationToken);
        var permissions = projection.MemberId.HasValue
            ? await _permissions.GetByMemberIdAsync(projection.MemberId.Value, cancellationToken)
            : new List<Domain.Enums.Permission>();

        return Result.Success(new GetMeDto(
            UserName:                projection.UserName,
            FullName:                projection.FullName,
            Email:                   projection.Email,
            PhoneNumber:             projection.PhoneNumber ?? string.Empty,
            ProfileImageUrl:         projection.ProfileImageUrl,
            Roles:                   roles.Select(r => r.RoleName).ToList(),
            Permissions:             permissions.Select(p => p.ToString()).ToList(),
            EmailConfirmed:          projection.EmailConfirmed,
            OnboardingCompleted:     projection.OnboardingCompleted,
            HasPassword:             projection.HasPassword,
            SpecializationNameEn:    projection.SpecializationNameEn,
            SpecializationNameAr:    projection.SpecializationNameAr,
            Gender:                  projection.Gender,
            StaffId:                 projection.MemberId,
            MemberId:                projection.DoctorInfoId,
            WeekStartDay:            projection.WeekStartDay,
            LastLoginAt:             projection.LastLoginAt,
            LastPasswordChangeAt:    projection.LastPasswordChangeAt
        ));
    }
}
