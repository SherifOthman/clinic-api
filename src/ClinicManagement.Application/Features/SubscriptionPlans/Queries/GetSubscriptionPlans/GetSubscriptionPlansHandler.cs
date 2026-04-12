using ClinicManagement.Application.Abstractions.Data;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries;

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, IEnumerable<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _uow;

    public GetSubscriptionPlansHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SubscriptionPlanDto>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var rows = await _uow.Reference.GetActiveSubscriptionPlansAsync(cancellationToken);

        return rows.Select(p => new SubscriptionPlanDto(
            p.Id, p.Name, p.NameAr, p.Description, p.DescriptionAr,
            p.MonthlyFee, p.YearlyFee, p.SetupFee,
            p.MaxBranches, p.MaxStaff, p.MaxPatientsPerMonth,
            p.MaxAppointmentsPerMonth, p.MaxInvoicesPerMonth, p.StorageLimitGB,
            p.HasInventoryManagement, p.HasReporting, p.HasAdvancedReporting,
            p.HasApiAccess, p.HasMultipleBranches, p.HasCustomBranding,
            p.HasPrioritySupport, p.HasBackupAndRestore, p.HasIntegrations,
            p.IsActive, p.IsPopular, p.DisplayOrder,
            p.Version, p.EffectiveDate, p.ExpiryDate));
    }
}
