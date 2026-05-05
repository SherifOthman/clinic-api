using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public record GetContactMessagesUnreadCountQuery : IRequest<Result<int>>;

public class GetContactMessagesUnreadCountHandler : IRequestHandler<GetContactMessagesUnreadCountQuery, Result<int>>
{
    private readonly IUnitOfWork _uow;
    public GetContactMessagesUnreadCountHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(GetContactMessagesUnreadCountQuery request, CancellationToken ct)
    {
        var count = await _uow.ContactMessages.CountUnreadAsync(ct);
        return Result.Success(count);
    }
}
