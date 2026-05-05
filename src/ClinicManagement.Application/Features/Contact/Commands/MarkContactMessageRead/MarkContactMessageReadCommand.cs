using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Commands;

public record MarkContactMessageReadCommand(Guid Id) : IRequest<Result>;

public class MarkContactMessageReadHandler : IRequestHandler<MarkContactMessageReadCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public MarkContactMessageReadHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(MarkContactMessageReadCommand request, CancellationToken ct)
    {
        var message = await _uow.ContactMessages.GetByIdAsync(request.Id, ct);

        if (message is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Message not found");

        if (message.IsRead) return Result.Success(); // already read — no-op

        message.IsRead = true;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
