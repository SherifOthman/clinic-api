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
        var profile = await uow.Users.GetProfileAsync(request.UserId, cancellationToken);
        if (profile is null)
            return Result.Failure<GetMeDto>(ErrorCodes.NOT_FOUND, "User not found");

        var roles     = await uow.Users.GetRolesByUserIdAsync(request.UserId, cancellationToken);
        var hasClinic = await uow.Users.HasClinicAsync(request.UserId, cancellationToken);
        var hasPassword = profile.HasPassword;

        string? specializationNameEn = null;
        string? specializationNameAr = null;
        Guid? staffId = null;
        Guid? memberId = null;
        string? appointmentType = null;
        int weekStartDay = 6; // default Saturday
        List<string> permissions = [];

        var member = await uow.Members.GetByUserIdAsync(request.UserId, cancellationToken);
        if (member is not null)
        {
            staffId = member.Id;
            var memberPermissions = await uow.Permissions.GetByMemberIdAsync(member.Id, cancellationToken);
            permissions = memberPermissions.Select(p => p.ToString()).ToList();
        }

        // Load clinic WeekStartDay (works for both owner and staff)
        var clinic = await uow.Clinics.GetByOwnerIdAsync(request.UserId, cancellationToken);
        if (clinic is null && member is not null)
        {
            var memberClinic = await uow.Clinics.GetByIdAsync(member.ClinicId, cancellationToken);
            weekStartDay = memberClinic?.WeekStartDay ?? 6;
        }
        else if (clinic is not null)
        {
            weekStartDay = clinic.WeekStartDay;
        }

        if (roles.Any(r => r.RoleName == UserRoles.Doctor))
        {
            var spec = await uow.Users.GetDoctorSpecializationAsync(request.UserId, cancellationToken);
            specializationNameEn = spec?.NameEn;
            specializationNameAr = spec?.NameAr;

            if (member is not null)
            {
                var memberWithDoctor = await uow.Members.GetByIdWithDoctorInfoAsync(member.Id, cancellationToken);
                if (memberWithDoctor?.DoctorInfo is not null)
                {
                    memberId = memberWithDoctor.DoctorInfo.Id;
                    appointmentType = memberWithDoctor.DoctorInfo.AppointmentType.ToString();
                }
            }
        }

        return Result.Success(new GetMeDto(
            UserName:             profile.UserName,
            FullName:             profile.FullName,
            Email:                profile.Email,
            PhoneNumber:          profile.PhoneNumber ?? string.Empty,
            ProfileImageUrl:      profile.ProfileImageUrl,
            Roles:                roles.Select(r => r.RoleName).ToList(),
            Permissions:          permissions,
            EmailConfirmed:       profile.EmailConfirmed,
            OnboardingCompleted:  hasClinic,
            HasPassword:          hasPassword,
            SpecializationNameEn: specializationNameEn,
            SpecializationNameAr: specializationNameAr,
            Gender:               profile.Gender,
            StaffId:              staffId,
            MemberId:             memberId,
            AppointmentType:      appointmentType,
            WeekStartDay:         weekStartDay
        ));
    }
}
