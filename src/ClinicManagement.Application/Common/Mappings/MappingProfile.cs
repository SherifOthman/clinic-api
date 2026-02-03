using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // ChronicDisease mapping - set Name and Description based on current language
        TypeAdapterConfig<ChronicDisease, ChronicDiseaseDto>
            .NewConfig()
            .Map(dest => dest.Name, src => src.NameEn) // Default to English, will be handled in service layer
            .Map(dest => dest.Description, src => src.DescriptionEn);
        
        // All other DTOs will be mapped automatically since properties have the same names
    }
}
