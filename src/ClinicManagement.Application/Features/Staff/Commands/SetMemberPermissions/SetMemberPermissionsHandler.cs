using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetMemberPermissionsHandler : IRequestHandler<SetMemberPermissionsCommand, Result>
{
    private readonly IClinicMemberRepository _members;
    private readonly IPermissionRepository   _permissions;
    private readonly IUnitOfWork             _uow;
    private readonly ICurrentUserService     _currentUser;
    private readonly IAuditWriter            _audit;

    public SetMemberPermissionsHandler(
        IClinicMemberRepository members,
        IPermissionRepository permissions,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuditWriter audit)
    {
        _members     = members;
        _permissions = permissions;
        _uow         = uow;
        _currentUser = currentUser;
        _audit       = audit;
    }

    public async Task<Result> Handle(SetMemberPermissionsCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var member = await _members.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null || member.ClinicId != clinicId)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Staff member not found");

        if (member.IsOwner)
            return Result.Failure(ErrorCodes.FORBIDDEN, "Owner permissions cannot be modified");

        var permissions = request.RawPermissions
            .Where(p => Enum.TryParse<Permission>(p, out _))
            .Select(p => Enum.Parse<Permission>(p))
            .ToList();

        var previousPermissions = await _permissions.GetByMemberIdAsync(request.MemberId, cancellationToken);
        await _permissions.SetPermissionsAsync(request.MemberId, permissions, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var added   = permissions.Except(previousPermissions).Select(p => p.ToString());
        var removed = previousPermissions.Except(permissions).Select(p => p.ToString());
        var detail  = $"Granted: [{string.Join(", ", added)}] | Revoked: [{string.Join(", ", removed)}]";

        await _audit.WriteEventAsync("PermissionsChanged", detail, ct: cancellationToken);

        return Result.Success();
    }
}
