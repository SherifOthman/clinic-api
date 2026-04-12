using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record GetMeQuery(Guid UserId) : IRequest<GetMeDto?>;

public record GetMeDto(
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? ProfileImageUrl,
    List<string> Roles,
    bool EmailConfirmed,
    bool OnboardingCompleted,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    string Gender
);
