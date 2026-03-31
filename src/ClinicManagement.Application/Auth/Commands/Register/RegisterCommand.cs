using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    bool IsMale,
    string PhoneNumber
) : IRequest<Result>;
