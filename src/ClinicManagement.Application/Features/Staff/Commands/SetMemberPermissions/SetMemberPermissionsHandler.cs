using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetMemberPermissionsHandler : IRequestHandler<SetMemberPermissionsCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditWriter _audit;

    public SetMemberPermissionsHandler(IUnitOfWork uow, ICurrentUserService currentUser, IAuditWriter audit)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _audit       = audit;
    }

    public async Task<Result> Handle(SetMemberPermissionsCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var member = await _uow.Members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        if (member.IsOwner)
            return Result.Failure(ErrorCodes.FORBIDDEN, "Owner permissions cannot be modified");

        // Parse and silently ignore unknown permission strings
        var permissions = request.RawPermissions
            .Where(p => Enum.TryParse<Permission>(p, out _))
            .Select(p => Enum.Parse<Permission>(p))
            .ToList();

        // Capture before state for diff — behavior can't do this
        var previousPermissions = await _uow.Permissions.GetByMemberIdAsync(request.MemberId, cancellationToken);
        await _uow.Permissions.SetPermissionsAsync(request.MemberId, permissions, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var added   = permissions.Except(previousPermissions).Select(p => p.ToString());
        var removed = previousPermissions.Except(permissions).Select(p => p.ToString());
        var detail  = $"Granted: [{string.Join(", ", added)}] | Revoked: [{string.Join(", ", removed)}]";

        // Manual audit — batch permission changes need a human-readable diff, not raw row inserts/deletes
        await _audit.WriteEventAsync("PermissionsChanged", detail, ct: cancellationToken);

        return Result.Success();
    }
}
