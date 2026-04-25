using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FullName,
    string userName,
    string? PhoneNumber,
    string Gender
) : IRequest<Result>;
