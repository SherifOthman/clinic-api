using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Commands;

public record MarkContactMessageReadCommand(Guid Id) : IRequest<Result>;

public class MarkContactMessageReadHandler : IRequestHandler<MarkContactMessageReadCommand, Result>
{
    private readonly IContactMessageRepository _contactMessages;
    private readonly IUnitOfWork               _uow;

    public MarkContactMessageReadHandler(IContactMessageRepository contactMessages, IUnitOfWork uow)
    {
        _contactMessages = contactMessages;
        _uow             = uow;
    }

    public async Task<Result> Handle(MarkContactMessageReadCommand request, CancellationToken ct)
    {
        var message = await _contactMessages.GetByIdAsync(request.Id, ct);

        if (message is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Message not found");

        if (message.IsRead) return Result.Success();

        message.IsRead = true;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
