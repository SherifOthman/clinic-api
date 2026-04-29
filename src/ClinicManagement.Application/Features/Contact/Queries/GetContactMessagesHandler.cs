using ClinicManagement.Application.Abstractions.Data;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public class GetContactMessagesHandler : IRequestHandler<GetContactMessagesQuery, List<ContactMessageDto>>
{
    private readonly IUnitOfWork _uow;

    public GetContactMessagesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ContactMessageDto>> Handle(GetContactMessagesQuery request, CancellationToken ct)
    {
        var messages = await _uow.ContactMessages.GetPagedAsync(request.Page, request.PageSize, ct);
        return messages.Select(m => new ContactMessageDto(
            m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.Company,
            m.Subject, m.Message, m.IsRead, m.CreatedAt
        )).ToList();
    }
}
