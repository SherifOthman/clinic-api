using Mapster;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.SubscriptionPlans.Queries;
using ClinicManagement.Application.Specializations.Queries;

namespace ClinicManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // SubscriptionPlan to SubscriptionPlanDto
        TypeAdapterConfig<SubscriptionPlan, SubscriptionPlanDto>
            .NewConfig();

        // Specialization to SpecializationDto
        TypeAdapterConfig<Specialization, SpecializationDto>
            .NewConfig();
    }
}
