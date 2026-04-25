using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetMemberPermissionsHandler : IRequestHandler<SetMemberPermissionsCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ISecurityAuditWriter _auditWriter;

    public SetMemberPermissionsHandler(IUnitOfWork uow, ICurrentUserService currentUser, ISecurityAuditWriter auditWriter)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _auditWriter = auditWriter;
    }

    public async Task<Result> Handle(SetMemberPermissionsCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var member = await _uow.Members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        if (member.IsOwner)
            return Result.Failure(ErrorCodes.FORBIDDEN, "Owner permissions cannot be modified");

        // Capture before state for diff — behavior can't do this
        var previousPermissions = await _uow.Permissions.GetByMemberIdAsync(request.MemberId, cancellationToken);
        await _uow.Permissions.SetPermissionsAsync(request.MemberId, request.Permissions, cancellationToken);

        var added   = request.Permissions.Except(previousPermissions).Select(p => p.ToString());
        var removed = previousPermissions.Except(request.Permissions).Select(p => p.ToString());
        var detail  = $"Granted: [{string.Join(", ", added)}] | Revoked: [{string.Join(", ", removed)}]";

        // Manual audit — needs DB diff that the behavior can't compute
        await _auditWriter.WriteAsync(
            _currentUser.UserId, _currentUser.FullName, _currentUser.Username, _currentUser.Email,
            _currentUser.Roles.FirstOrDefault(), clinicId,
            "PermissionsChanged", detail, cancellationToken);

        return Result.Success();
    }
}
