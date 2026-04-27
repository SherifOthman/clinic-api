using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class GetMeHandler(IUnitOfWork uow) : IRequestHandler<GetMeQuery, GetMeDto?>
{
    public async Task<GetMeDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var profile = await uow.Users.GetProfileAsync(request.UserId, cancellationToken);
        if (profile is null) return null;

        var roles     = await uow.Users.GetRolesByUserIdAsync(request.UserId, cancellationToken);
        var hasClinic = await uow.Users.HasClinicAsync(request.UserId, cancellationToken);

        string? specializationNameEn = null;
        string? specializationNameAr = null;
        Guid? staffId = null;
        List<string> permissions = [];
        
        var member = await uow.Members.GetByUserIdAsync(request.UserId, cancellationToken);
        if (member is not null)
        {
            staffId = member.Id;
            var memberPermissions = await uow.Permissions.GetByMemberIdAsync(member.Id, cancellationToken);
            permissions = memberPermissions.Select(p => p.ToString()).ToList();
        }

        if (roles.Any(r => r.RoleName == UserRoles.Doctor))
        {
            var spec = await uow.Users.GetDoctorSpecializationAsync(request.UserId, cancellationToken);
            specializationNameEn = spec?.NameEn;
            specializationNameAr = spec?.NameAr;
        }

        return new GetMeDto(
            UserName:             profile.UserName,
            FullName:             profile.FullName,
            Email:                profile.Email,
            PhoneNumber:          profile.PhoneNumber ?? string.Empty,
            ProfileImageUrl:      profile.ProfileImageUrl,
            Roles:                roles.Select(r => r.RoleName).ToList(),
            Permissions:          permissions,
            EmailConfirmed:       profile.EmailConfirmed,
            OnboardingCompleted:  hasClinic,
            SpecializationNameEn: specializationNameEn,
            SpecializationNameAr: specializationNameAr,
            Gender:               profile.Gender,
            StaffId:              staffId
        );
    }
}
