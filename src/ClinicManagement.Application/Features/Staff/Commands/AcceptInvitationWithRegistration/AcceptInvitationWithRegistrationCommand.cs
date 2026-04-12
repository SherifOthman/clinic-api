using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public record AcceptInvitationWithRegistrationCommand(
    string Token,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string PhoneNumber,
    string Gender
) : IRequest<Result>;
