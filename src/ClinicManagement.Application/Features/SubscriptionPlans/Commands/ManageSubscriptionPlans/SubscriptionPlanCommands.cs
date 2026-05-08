using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands;

public record SubscriptionPlanBody(
    string Name, string NameAr, string Description, string DescriptionAr,
    decimal MonthlyFee, decimal YearlyFee, decimal SetupFee,
    int MaxBranches, int MaxStaff, int MaxPatientsPerMonth,
    int MaxAppointmentsPerMonth, int MaxInvoicesPerMonth, int StorageLimitGB,
    bool HasInventoryManagement, bool HasReporting, bool HasAdvancedReporting,
    bool HasApiAccess, bool HasMultipleBranches, bool HasCustomBranding,
    bool HasPrioritySupport, bool HasBackupAndRestore, bool HasIntegrations,
    bool IsActive, bool IsPopular, int DisplayOrder);

public record CreateSubscriptionPlanCommand(SubscriptionPlanBody Plan) : IRequest<Result<Guid>>;

public class CreateSubscriptionPlanHandler : IRequestHandler<CreateSubscriptionPlanCommand, Result<Guid>>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public CreateSubscriptionPlanHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result<Guid>> Handle(CreateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var p = req.Plan;
        var entity = new SubscriptionPlan
        {
            Name = p.Name.Trim(), NameAr = p.NameAr.Trim(),
            Description = p.Description.Trim(), DescriptionAr = p.DescriptionAr.Trim(),
            MonthlyFee = p.MonthlyFee, YearlyFee = p.YearlyFee, SetupFee = p.SetupFee,
            MaxBranches = p.MaxBranches, MaxStaff = p.MaxStaff,
            MaxPatientsPerMonth = p.MaxPatientsPerMonth,
            MaxAppointmentsPerMonth = p.MaxAppointmentsPerMonth,
            MaxInvoicesPerMonth = p.MaxInvoicesPerMonth,
            StorageLimitGB = p.StorageLimitGB,
            HasInventoryManagement = p.HasInventoryManagement,
            HasReporting = p.HasReporting, HasAdvancedReporting = p.HasAdvancedReporting,
            HasApiAccess = p.HasApiAccess, HasMultipleBranches = p.HasMultipleBranches,
            HasCustomBranding = p.HasCustomBranding, HasPrioritySupport = p.HasPrioritySupport,
            HasBackupAndRestore = p.HasBackupAndRestore, HasIntegrations = p.HasIntegrations,
            IsActive = p.IsActive, IsPopular = p.IsPopular, DisplayOrder = p.DisplayOrder,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
        };
        _reference.AddSubscriptionPlan(entity);
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success(entity.Id);
    }
}

public record UpdateSubscriptionPlanCommand(Guid Id, SubscriptionPlanBody Plan) : IRequest<Result>;

public class UpdateSubscriptionPlanHandler : IRequestHandler<UpdateSubscriptionPlanCommand, Result>
{
    private readonly IReferenceRepository _reference;
    private readonly IUnitOfWork          _uow;

    public UpdateSubscriptionPlanHandler(IReferenceRepository reference, IUnitOfWork uow)
    {
        _reference = reference;
        _uow       = uow;
    }

    public async Task<Result> Handle(UpdateSubscriptionPlanCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetSubscriptionPlanByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Subscription plan not found");

        var p = req.Plan;
        entity.Name = p.Name.Trim(); entity.NameAr = p.NameAr.Trim();
        entity.Description = p.Description.Trim(); entity.DescriptionAr = p.DescriptionAr.Trim();
        entity.MonthlyFee = p.MonthlyFee; entity.YearlyFee = p.YearlyFee; entity.SetupFee = p.SetupFee;
        entity.MaxBranches = p.MaxBranches; entity.MaxStaff = p.MaxStaff;
        entity.MaxPatientsPerMonth = p.MaxPatientsPerMonth;
        entity.MaxAppointmentsPerMonth = p.MaxAppointmentsPerMonth;
        entity.MaxInvoicesPerMonth = p.MaxInvoicesPerMonth;
        entity.StorageLimitGB = p.StorageLimitGB;
        entity.HasInventoryManagement = p.HasInventoryManagement;
        entity.HasReporting = p.HasReporting; entity.HasAdvancedReporting = p.HasAdvancedReporting;
        entity.HasApiAccess = p.HasApiAccess; entity.HasMultipleBranches = p.HasMultipleBranches;
        entity.HasCustomBranding = p.HasCustomBranding; entity.HasPrioritySupport = p.HasPrioritySupport;
        entity.HasBackupAndRestore = p.HasBackupAndRestore; entity.HasIntegrations = p.HasIntegrations;
        entity.IsActive = p.IsActive; entity.IsPopular = p.IsPopular; entity.DisplayOrder = p.DisplayOrder;

        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}

public record ToggleSubscriptionPlanCommand(Guid Id) : IRequest<Result>;

public class ToggleSubscriptionPlanHandler : IRequestHandler<ToggleSubscriptionPlanCommand, Result>
{
    private readonly IReferenceRepository      _reference;
    private readonly ISubscriptionPlanRepository _subscriptionPlans;
    private readonly IUnitOfWork               _uow;

    public ToggleSubscriptionPlanHandler(
        IReferenceRepository reference,
        ISubscriptionPlanRepository subscriptionPlans,
        IUnitOfWork uow)
    {
        _reference         = reference;
        _subscriptionPlans = subscriptionPlans;
        _uow               = uow;
    }

    public async Task<Result> Handle(ToggleSubscriptionPlanCommand req, CancellationToken ct)
    {
        var entity = await _reference.GetSubscriptionPlanByIdAsync(req.Id, ct);
        if (entity is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Subscription plan not found");

        if (entity.IsActive)
        {
            var activeCount = await _subscriptionPlans.CountActiveSubscribersAsync(req.Id, ct);
            if (activeCount > 0)
                return Result.Failure(
                    ErrorCodes.OPERATION_NOT_ALLOWED,
                    $"Cannot deactivate '{entity.Name}' — {activeCount} clinic{(activeCount == 1 ? " is" : "s are")} currently subscribed to this plan.");
        }

        entity.IsActive = !entity.IsActive;
        await _uow.SaveChangesAsync(ct);
        _reference.InvalidateCache();
        return Result.Success();
    }
}
