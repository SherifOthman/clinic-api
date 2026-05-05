using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public record GetContactMessagesQuery(
    int PageNumber = 1,
    int PageSize = 20
) : PaginatedQuery(PageNumber, PageSize), IRequest<Result<PaginatedResult<ContactMessageDto>>>;

public record ContactMessageDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Company,
    string Subject,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAt
);
