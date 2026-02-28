using ClinicManagement.Application.Auth.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, GetMeDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserName, src => src.UserName!)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email!)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber ?? string.Empty)
            .Map(dest => dest.ProfileImageUrl, src => src.ProfileImageUrl)
            .Map(dest => dest.EmailConfirmed, src => src.EmailConfirmed)
            .Ignore(dest => dest.Roles)
            .Ignore(dest => dest.OnboardingCompleted);
    }
}
