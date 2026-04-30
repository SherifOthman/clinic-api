using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Queries;

public record GetStaffDetailQuery(Guid StaffId) : IRequest<Result<StaffDetailDto>>;

public record StaffDetailDto(
    Guid Id,
    string FullName,
    string Gender,
    string? Email,
    string? PhoneNumber,
    DateTimeOffset JoinDate,
    string? ProfileImageUrl,
    bool IsActive,
    IEnumerable<StaffRoleDto> Roles,
    DoctorDetailDto? DoctorProfile
);

public record DoctorDetailDto(
    Guid DoctorProfileId,
    string SpecializationNameEn,
    string SpecializationNameAr,
    bool CanSelfManageSchedule,
    string AppointmentType
);
