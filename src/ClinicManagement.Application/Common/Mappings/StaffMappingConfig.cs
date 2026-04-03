using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Domain.Entities;
using Mapster;

namespace ClinicManagement.Application.Common.Mappings;

public class StaffMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // InvitationDto is the flat list DTO — no AcceptedAt/AcceptedBy
        config.NewConfig<StaffInvitation, InvitationDto>()
            .MapWith(src => new InvitationDto(
                src.Id,
                src.Email,
                src.Role,
                src.Specialization != null ? src.Specialization.NameEn : null,
                src.IsAccepted ? InvitationStatus.Accepted
                    : src.IsCanceled ? InvitationStatus.Canceled
                    : src.ExpiresAt <= DateTime.UtcNow ? InvitationStatus.Expired
                    : InvitationStatus.Pending,
                src.CreatedAt,
                src.ExpiresAt,
                src.CreatedByUser != null ? src.CreatedByUser.FullName : "Unknown"
            ));
    }
}
