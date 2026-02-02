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
        // Mappings for properties with different names
        TypeAdapterConfig<User, UserDto>
            .NewConfig()
            .Map(dest => dest.Username, src => src.UserName);
        
        TypeAdapterConfig<RegisterCommand, User>
            .NewConfig()
            .Map(dest => dest.UserName, src => src.Username);

        // Mappings that need custom logic
        TypeAdapterConfig<Patient, PatientDto>
            .NewConfig()
            .Map(dest => dest.Age, src => src.GetAge());

        TypeAdapterConfig<Clinic, ClinicDto>
            .NewConfig()
            .Map(dest => dest.SubscriptionPlanName, src => src.SubscriptionPlan.Name)
            .Map(dest => dest.UserCount, src => src.Users.Count)
            .Map(dest => dest.PatientCount, src => src.Patients.Count);

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
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.UpdatedBy)
            .Ignore(dest => dest.DeletedBy);

        // UserClinic to UserClinicDto mapping
        TypeAdapterConfig<UserClinic, UserClinicDto>
            .NewConfig()
            .Map(dest => dest.ClinicName, src => src.Clinic.Name)
            .Map(dest => dest.SubscriptionPlan, src => src.Clinic.SubscriptionPlan)
            .Ignore(dest => dest.IsCurrent); // Will be set manually in handler
        
        // All other DTOs (SubscriptionPlan, etc.) 
        // will be mapped automatically since properties have the same names
    }
}
