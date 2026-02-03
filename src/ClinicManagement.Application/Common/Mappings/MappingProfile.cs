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
        
        // User to UserDto mapping is handled automatically by Mapster
        // since property names match between User entity and UserDto
    }
}
