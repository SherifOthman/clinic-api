using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Commands;

public record SendContactMessageCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Company,
    string Subject,
    string Message
) : IRequest<Result>;
