using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;
using ClinicManagement.Application.Features.Staff.Commands.InviteStaff;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        // Only configure mappings where property names differ or custom logic is needed
        
        // User → UserDto: UserName → Username (different property names)
        TypeAdapterConfig<User, UserDto>
            .NewConfig()
            .Map(dest => dest.Username, src => src.UserName);
        
        // RegisterCommand → User: Username → UserName (different property names)
        TypeAdapterConfig<RegisterCommand, User>
            .NewConfig()
            .Map(dest => dest.UserName, src => src.Username);

        // InviteStaffCommand → User: Custom logic for email and defaults
        TypeAdapterConfig<InviteStaffCommand, User>
            .NewConfig()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.EmailConfirmed, src => false)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

        // CompleteOnboardingCommand → Clinic: Property name differences and defaults
        TypeAdapterConfig<CompleteOnboardingCommand, Clinic>
            .NewConfig()
            .Map(dest => dest.Name, src => src.ClinicName)
            .Map(dest => dest.PhoneNumber, src => src.ClinicPhone)
            .Map(dest => dest.OnboardingCompleted, src => true)
            .Map(dest => dest.OnboardingStep, src => "completed")
            .Map(dest => dest.IsActive, src => true)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);
        
        // All other mappings use Mapster's convention-based mapping:
        // - Clinic → ClinicDto (all properties match by name)
        // - Patient → PatientDto (all properties match by name)  
        // - Doctor → DoctorDto (all properties match by name)
        // - Appointment → AppointmentDto (all properties match by name)
        // No explicit configuration needed - Mapster handles these automatically!
    }
}
