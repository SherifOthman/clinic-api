using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public class GetContactMessagesHandler : IRequestHandler<GetContactMessagesQuery, Result<PaginatedResult<ContactMessageDto>>>
{
    private readonly IContactMessageRepository _contactMessages;

    public GetContactMessagesHandler(IContactMessageRepository contactMessages) => _contactMessages = contactMessages;

    public async Task<Result<PaginatedResult<ContactMessageDto>>> Handle(GetContactMessagesQuery request, CancellationToken ct)
    {
        var result = await _contactMessages.GetPagedAsync(request.PageNumber, request.PageSize, ct);

        var dtos = result.Items.Select(m => new ContactMessageDto(
            m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.Company,
            m.Subject, m.Message, m.IsRead, m.CreatedAt
        )).ToList();

        return Result.Success(PaginatedResult<ContactMessageDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
