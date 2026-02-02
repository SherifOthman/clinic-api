using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // No custom mappings needed for UserName since both use the same property name

        // Mappings that need custom logic
        TypeAdapterConfig<Patient, PatientDto>
            .NewConfig()
            .Map(dest => dest.Age, src => src.GetAge());

        TypeAdapterConfig<Clinic, ClinicDto>
            .NewConfig()
            .Map(dest => dest.SubscriptionPlanName, src => src.SubscriptionPlan != null ? src.SubscriptionPlan.Name : string.Empty)
            .Map(dest => dest.UserCount, src => src.Users != null ? src.Users.Count : 0)
            .Map(dest => dest.PatientCount, src => src.Patients != null ? src.Patients.Count : 0);

        // GeoNames mappings - simplified without hardcoded phone codes
        TypeAdapterConfig<GeoNamesLocationDto, CountryDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.GeoNameId)
            .Map(dest => dest.Code, src => src.CountryCode)
            .Map(dest => dest.PhoneCode, src => ""); // Will be populated from external API if needed

        TypeAdapterConfig<GeoNamesLocationDto, StateDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.GeoNameId)
            .Map(dest => dest.Name, src => src.AdminName1 ?? src.Name);

        TypeAdapterConfig<GeoNamesLocationDto, CityDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.GeoNameId)
            .Map(dest => dest.StateId, src => string.IsNullOrEmpty(src.AdminName1) ? (int?)null : src.AdminName1.GetHashCode());

        // ChronicDisease mapping - set Name and Description based on current language
        TypeAdapterConfig<ChronicDisease, ChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.NameEn) // Default to English, will be handled in service layer
            .Map(dest => dest.Description, src => src.DescriptionEn);

        // UpdatePatientCommand to Patient mapping - ignore collections as they need special handling
        TypeAdapterConfig<UpdatePatientCommand, Patient>
            .NewConfig()
            .Ignore(dest => dest.PhoneNumbers)
            .Ignore(dest => dest.ChronicDiseases)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ClinicId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
#pragma warning disable CS8603 // Possible null reference return
            .Ignore(dest => dest.DeletedAt)
#pragma warning restore CS8603 // Possible null reference return
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.UpdatedBy)
            .Ignore(dest => dest.DeletedBy)
            .Ignore(dest => dest.Clinic)
            .Ignore(dest => dest.IsDeleted);

        // UserClinic to UserClinicDto mapping
        TypeAdapterConfig<UserClinic, UserClinicDto>
            .NewConfig()
            .Map(dest => dest.ClinicName, src => src.Clinic != null ? src.Clinic.Name : string.Empty)
            .Map(dest => dest.SubscriptionPlan, src => src.Clinic != null ? src.Clinic.SubscriptionPlan : null)
            .Ignore(dest => dest.IsCurrent); // Will be set manually in handler
        
        // All other DTOs (SubscriptionPlan, etc.) 
        // will be mapped automatically since properties have the same names
    }
}
