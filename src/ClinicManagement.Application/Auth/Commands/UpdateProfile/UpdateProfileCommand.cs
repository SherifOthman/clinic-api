using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest<Result>;
