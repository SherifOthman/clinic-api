using ClinicManagement.Application.Staff.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class StaffMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Entities.Staff, StaffDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.User!.Id)
            .Map(dest => dest.FullName, src => src.User!.FullName)
            .Map(dest => dest.Email, src => src.User!.Email!)
            .Map(dest => dest.PhoneNumber, src => src.User!.PhoneNumber)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.HireDate, src => src.HireDate)
            .Ignore(dest => dest.Role);

        config.NewConfig<StaffInvitation, PendingInvitationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Role, src => src.Role)
            .Map(dest => dest.Token, src => src.InvitationToken)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.InvitedByUserName, src => src.CreatedByUser != null ? src.CreatedByUser.FullName : "Unknown");
    }
}
