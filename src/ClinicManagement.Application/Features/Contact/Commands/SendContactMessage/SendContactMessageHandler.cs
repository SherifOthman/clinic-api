using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Contact.Commands;

public class SendContactMessageHandler : IRequestHandler<SendContactMessageCommand, Result>
{
    private readonly IContactMessageRepository _contactMessages;
    private readonly IUnitOfWork               _uow;

    public SendContactMessageHandler(IContactMessageRepository contactMessages, IUnitOfWork uow)
    {
        _contactMessages = contactMessages;
        _uow             = uow;
    }

    public async Task<Result> Handle(SendContactMessageCommand request, CancellationToken ct)
    {
        await _contactMessages.AddAsync(new ContactMessage
        {
            FirstName = request.FirstName.Trim(),
            LastName  = request.LastName.Trim(),
            Email     = request.Email.Trim().ToLowerInvariant(),
            Phone     = request.Phone?.Trim(),
            Company   = request.Company?.Trim(),
            Subject   = request.Subject.Trim(),
            Message   = request.Message.Trim(),
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
