namespace ClinicManagement.Application.Features.Reference.QueryModels;

/// <summary>Flat projection rows returned by the reference data repository (cached).</summary>

public record ChronicDiseaseRow(Guid Id, string NameEn, string NameAr);

public record SpecializationRow(
    Guid Id, string NameEn, string NameAr,
    string? DescriptionEn, string? DescriptionAr);

public record SubscriptionPlanRow(
    Guid Id, string Name, string NameAr, string Description, string DescriptionAr,
    decimal MonthlyFee, decimal YearlyFee, decimal SetupFee,
    int MaxBranches, int MaxStaff, int MaxPatientsPerMonth,
    int MaxAppointmentsPerMonth, int MaxInvoicesPerMonth, int StorageLimitGB,
    bool HasInventoryManagement, bool HasReporting, bool HasAdvancedReporting,
    bool HasApiAccess, bool HasMultipleBranches, bool HasCustomBranding,
    bool HasPrioritySupport, bool HasBackupAndRestore, bool HasIntegrations,
    bool IsActive, bool IsPopular, int DisplayOrder,
    int Version, DateOnly EffectiveDate, DateOnly? ExpiryDate);
