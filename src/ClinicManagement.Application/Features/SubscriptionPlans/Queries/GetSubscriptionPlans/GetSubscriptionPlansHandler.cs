using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries;

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IReferenceRepository _reference;

    public GetSubscriptionPlansHandler(IReferenceRepository reference) => _reference = reference;

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var rows = await _reference.GetActiveSubscriptionPlansAsync(cancellationToken);

        var list = rows.Select(p => new SubscriptionPlanDto(
            p.Id, p.Name, p.NameAr, p.Description, p.DescriptionAr,
            p.MonthlyFee, p.YearlyFee, p.SetupFee,
            p.MaxBranches, p.MaxStaff, p.MaxPatientsPerMonth,
            p.MaxAppointmentsPerMonth, p.MaxInvoicesPerMonth, p.StorageLimitGB,
            p.HasInventoryManagement, p.HasReporting, p.HasAdvancedReporting,
            p.HasApiAccess, p.HasMultipleBranches, p.HasCustomBranding,
            p.HasPrioritySupport, p.HasBackupAndRestore, p.HasIntegrations,
            p.IsActive, p.IsPopular, p.DisplayOrder,
            p.Version, p.EffectiveDate, p.ExpiryDate)).ToList();

        return Result.Success(list);
    }
}
