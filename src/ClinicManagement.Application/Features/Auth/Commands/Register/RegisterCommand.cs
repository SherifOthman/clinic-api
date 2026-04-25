using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FullName,
    string UserName,
    string Email,
    string Password,
    string Gender,
    string PhoneNumber
) : IRequest<Result>;
