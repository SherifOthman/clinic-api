using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeDto?>
{
    private readonly IUnitOfWork _uow;

    public GetMeHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<GetMeDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var profile = await _uow.Users.GetProfileAsync(request.UserId, cancellationToken);
        if (profile is null) return null;

        var roles     = await _uow.Users.GetRolesByUserIdAsync(request.UserId, cancellationToken);
        var hasClinic = await _uow.Users.HasClinicAsync(request.UserId, cancellationToken);

        string? specializationNameEn = null;
        string? specializationNameAr = null;
        Guid? staffId = null;
        if (roles.Any(r => r.RoleName == Roles.Doctor))
        {
            var spec = await _uow.Users.GetDoctorSpecializationAsync(request.UserId, cancellationToken);
            specializationNameEn = spec?.NameEn;
            specializationNameAr = spec?.NameAr;
            var member = await _uow.Members.GetByUserIdAsync(request.UserId, cancellationToken);
            staffId = member?.Id;        }

        return new GetMeDto(
            UserName:             profile.UserName,
            FirstName:            profile.FirstName,
            LastName:             profile.LastName,
            Email:                profile.Email,
            PhoneNumber:          profile.PhoneNumber ?? string.Empty,
            ProfileImageUrl:      profile.ProfileImageUrl,
            Roles:                roles.Select(r => r.RoleName).ToList(),
            EmailConfirmed:       profile.EmailConfirmed,
            OnboardingCompleted:  hasClinic,
            SpecializationNameEn: specializationNameEn,
            SpecializationNameAr: specializationNameAr,
            Gender:               profile.Gender,
            StaffId:              staffId
        );
    }
}
