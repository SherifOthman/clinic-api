using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Permissions.Commands;

public record SetRoleDefaultPermissionsCommand(string Role, List<string> Permissions)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEvent   => "RoleDefaultPermissionsChanged";
    public string? AuditDetail => $"Role: {Role} | Permissions: [{string.Join(", ", Permissions)}]";
}

public class SetRoleDefaultPermissionsValidator : AbstractValidator<SetRoleDefaultPermissionsCommand>
{
    public SetRoleDefaultPermissionsValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<ClinicMemberRole>(r, out _))
            .WithMessage("Invalid role. Must be one of: Doctor, Receptionist, Owner");

        RuleFor(x => x.Permissions)
            .NotNull()
            .Must(list => list.All(p => Enum.TryParse<Permission>(p, out _)))
            .WithMessage("One or more permissions are invalid");

        // Owner must always keep full permissions — prevent lockout
        RuleFor(x => x)
            .Must(x =>
            {
                if (!Enum.TryParse<ClinicMemberRole>(x.Role, out var role)) return true;
                if (role != ClinicMemberRole.Owner) return true;
                var required = DefaultPermissions.Owner;
                var provided = x.Permissions.Select(p => Enum.Parse<Permission>(p)).ToHashSet();
                return required.All(p => provided.Contains(p));
            })
            .WithMessage("Owner role must retain all default permissions");
    }
}

public class SetRoleDefaultPermissionsHandler : IRequestHandler<SetRoleDefaultPermissionsCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public SetRoleDefaultPermissionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(SetRoleDefaultPermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = Enum.Parse<ClinicMemberRole>(request.Role);
        var permissions = request.Permissions
            .Select(p => Enum.Parse<Permission>(p))
            .Distinct()
            .ToList();

        await _uow.Permissions.SetDefaultsForRoleAsync(role, permissions, cancellationToken);
        return Result.Success();
    }
}
