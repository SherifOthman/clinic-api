using ClinicManagement.Application.SubscriptionPlans.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class SubscriptionPlanMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SubscriptionPlan, SubscriptionPlanDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.NameAr, src => src.NameAr)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.DescriptionAr, src => src.DescriptionAr)
            .Map(dest => dest.MonthlyFee, src => src.MonthlyFee)
            .Map(dest => dest.YearlyFee, src => src.YearlyFee)
            .Map(dest => dest.SetupFee, src => src.SetupFee)
            .Map(dest => dest.MaxBranches, src => src.MaxBranches)
            .Map(dest => dest.MaxStaff, src => src.MaxStaff)
            .Map(dest => dest.MaxPatientsPerMonth, src => src.MaxPatientsPerMonth)
            .Map(dest => dest.MaxAppointmentsPerMonth, src => src.MaxAppointmentsPerMonth)
            .Map(dest => dest.MaxInvoicesPerMonth, src => src.MaxInvoicesPerMonth)
            .Map(dest => dest.StorageLimitGB, src => src.StorageLimitGB)
            .Map(dest => dest.HasInventoryManagement, src => src.HasInventoryManagement)
            .Map(dest => dest.HasReporting, src => src.HasReporting)
            .Map(dest => dest.HasAdvancedReporting, src => src.HasAdvancedReporting)
            .Map(dest => dest.HasApiAccess, src => src.HasApiAccess)
            .Map(dest => dest.HasMultipleBranches, src => src.HasMultipleBranches)
            .Map(dest => dest.HasCustomBranding, src => src.HasCustomBranding)
            .Map(dest => dest.HasPrioritySupport, src => src.HasPrioritySupport)
            .Map(dest => dest.HasBackupAndRestore, src => src.HasBackupAndRestore)
            .Map(dest => dest.HasIntegrations, src => src.HasIntegrations)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.IsPopular, src => src.IsPopular)
            .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.EffectiveDate, src => src.EffectiveDate)
            .Map(dest => dest.ExpiryDate, src => src.ExpiryDate);
    }
}
