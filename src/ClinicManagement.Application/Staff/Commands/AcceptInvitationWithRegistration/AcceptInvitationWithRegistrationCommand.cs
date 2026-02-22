using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Staff.Commands;

public record AcceptInvitationWithRegistrationCommand(
    string Token,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber
) : IRequest<Result>;
