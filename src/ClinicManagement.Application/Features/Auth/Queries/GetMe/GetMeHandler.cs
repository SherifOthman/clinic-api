using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class GetMeHandler(IUnitOfWork uow) : IRequestHandler<GetMeQuery, Result<GetMeDto>>
{
    public async Task<Result<GetMeDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        // Single query: profile + member + clinic week start day + doctor info
        var projection = await uow.Users.GetMeProjectionAsync(request.UserId, cancellationToken);
        if (projection is null)
            return Result.Failure<GetMeDto>(ErrorCodes.NOT_FOUND, "User not found");

        // Roles and permissions are in separate tables — run in parallel
        var rolesTask = uow.Users.GetRolesByUserIdAsync(request.UserId, cancellationToken);
        var permissionsTask = projection.MemberId.HasValue
            ? uow.Permissions.GetByMemberIdAsync(projection.MemberId.Value, cancellationToken)
            : Task.FromResult(new List<Domain.Enums.Permission>());

        await Task.WhenAll(rolesTask, permissionsTask);

        var roles       = rolesTask.Result;
        var permissions = permissionsTask.Result;

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
