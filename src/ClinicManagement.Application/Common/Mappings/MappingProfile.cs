using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

/// <summary>
/// Centralized mapping configuration for the application.
/// Only contains mappings that are actually used.
/// </summary>
public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ChronicDisease to ChronicDiseaseDto mapping
        // Maps language-specific fields and sets default display values
        TypeAdapterConfig<ChronicDisease, ChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.NameEn) // Default to English
            .Map(dest => dest.Description, src => src.DescriptionEn); // Default to English
        
        // Specialization to SpecializationDto mapping
        // Automatic mapping since property names match
        TypeAdapterConfig<Specialization, SpecializationDto>
            .NewConfig();
        
        // Doctor to DoctorDto mapping
        // Includes specialization navigation property
        TypeAdapterConfig<Doctor, DoctorDto>
            .NewConfig()
            .Map(dest => dest.Specialization, src => src.Specialization);
        
        // ClinicPatientChronicDisease to ClinicPatientChronicDiseaseDto mapping
        // Includes chronic disease navigation property
        TypeAdapterConfig<ClinicPatientChronicDisease, ClinicPatientChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.ChronicDisease, src => src.ChronicDisease);
        
        // Appointment to AppointmentDto mapping
        // Includes patient and doctor names
        TypeAdapterConfig<Appointment, AppointmentDto>
            .NewConfig()
            .Map(dest => dest.PatientName, src => src.ClinicPatient.FullName)
            .Map(dest => dest.DoctorName, src => src.Doctor.User.FullName)
            .Map(dest => dest.AppointmentType, src => src.AppointmentType)
            .Map(dest => dest.RemainingAmount, src => src.FinalPrice - src.DiscountAmount - src.PaidAmount);
        
        // User to UserDto mapping is handled automatically by Mapster
        // since property names match between User entity and UserDto
    }
}
