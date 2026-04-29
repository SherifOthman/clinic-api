using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record GetMeQuery(Guid UserId) : IRequest<GetMeDto?>;

public record GetMeDto(
    string UserName,
    string FullName,
    string Email,
    string PhoneNumber,
    string? ProfileImageUrl,
    List<string> Roles,
    List<string> Permissions,
    bool EmailConfirmed,
    bool OnboardingCompleted,
    bool HasPassword,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    string Gender,
    Guid? StaffId
);
