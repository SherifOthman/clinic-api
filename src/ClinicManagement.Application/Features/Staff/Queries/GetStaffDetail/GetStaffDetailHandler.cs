using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public class GetStaffDetailHandler : IRequestHandler<GetStaffDetailQuery, Result<StaffDetailDto>>
{
    private readonly IUnitOfWork _uow;
    public GetStaffDetailHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<StaffDetailDto>> Handle(GetStaffDetailQuery request, CancellationToken cancellationToken)
    {
        var member = await _uow.Members.GetDetailAsync(request.StaffId, cancellationToken);
        if (member is null)
            return Result.Failure<StaffDetailDto>(ErrorCodes.NOT_FOUND, "Staff member not found");

        var roleRows = await _uow.Members.GetRolesByUserIdsAsync([member.UserId], cancellationToken);
        var roles    = roleRows.Select(r => new StaffRoleDto(r.RoleName)).ToList();

        DoctorDetailDto? doctorProfile = null;
        if (member.DoctorProfile is not null)
            doctorProfile = new DoctorDetailDto(
                member.DoctorProfile.Id,
                member.DoctorProfile.SpecializationNameEn,
                member.DoctorProfile.SpecializationNameAr,
                member.DoctorProfile.CanSelfManageSchedule,
                member.DoctorProfile.AppointmentType.ToString());

        return Result.Success(new StaffDetailDto(
            member.Id, member.FullName, member.Gender, member.Email,
            member.PhoneNumber, member.CreatedAt, member.ProfileImageUrl,
            member.IsActive, roles, doctorProfile));
    }
}
