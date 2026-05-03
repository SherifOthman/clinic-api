using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public class GetContactMessagesHandler : IRequestHandler<GetContactMessagesQuery, Result<PaginatedResult<ContactMessageDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetContactMessagesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<ContactMessageDto>>> Handle(GetContactMessagesQuery request, CancellationToken ct)
    {
        var result = await _uow.ContactMessages.GetPagedAsync(request.PageNumber, request.PageSize, ct);

        var dtos = result.Items.Select(m => new ContactMessageDto(
            m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.Company,
            m.Subject, m.Message, m.IsRead, m.CreatedAt
        )).ToList();

        return Result.Success(PaginatedResult<ContactMessageDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
