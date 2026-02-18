using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<UpdateProfileResult>;
