using ClinicManagement.Application.Auth.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, GetMeDto>()
            .MapToConstructor(true)
            .Map(dest => dest.UserName, src => src.UserName!)
            .Map(dest => dest.Email, src => src.Email!)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber ?? string.Empty)
            .Map(dest => dest.Roles, src => new List<string>())
            .Map(dest => dest.OnboardingCompleted, src => false);
    }
}
