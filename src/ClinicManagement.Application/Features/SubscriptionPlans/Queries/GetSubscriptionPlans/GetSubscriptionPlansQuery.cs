using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries;

public record GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanDto>>>;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string NameAr,
    string Description,
    string DescriptionAr,
    decimal MonthlyFee,
    decimal YearlyFee,
    decimal SetupFee,
    int MaxBranches,
    int MaxStaff,
    int MaxPatientsPerMonth,
    int MaxAppointmentsPerMonth,
    int MaxInvoicesPerMonth,
    int StorageLimitGB,
    bool HasInventoryManagement,
    bool HasReporting,
    bool HasAdvancedReporting,
    bool HasApiAccess,
    bool HasMultipleBranches,
    bool HasCustomBranding,
    bool HasPrioritySupport,
    bool HasBackupAndRestore,
    bool HasIntegrations,
    bool IsActive,
    bool IsPopular,
    int DisplayOrder,
    int Version,
    DateOnly EffectiveDate,
    DateOnly? ExpiryDate);
