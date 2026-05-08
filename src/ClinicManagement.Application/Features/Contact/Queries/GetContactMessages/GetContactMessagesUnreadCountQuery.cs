using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Queries;

public record GetContactMessagesUnreadCountQuery : IRequest<Result<int>>;

public class GetContactMessagesUnreadCountHandler : IRequestHandler<GetContactMessagesUnreadCountQuery, Result<int>>
{
    private readonly IContactMessageRepository _contactMessages;

    public GetContactMessagesUnreadCountHandler(IContactMessageRepository contactMessages)
        => _contactMessages = contactMessages;

    public async Task<Result<int>> Handle(GetContactMessagesUnreadCountQuery request, CancellationToken ct)
    {
        var count = await _contactMessages.CountUnreadAsync(ct);
        return Result.Success(count);
    }
}
