using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Tests.Builders;

public class StaffInvitationBuilder
{
    private Guid _clinicId = Guid.NewGuid();
    private string _email = "doctor@test.com";
    private string _role = "Doctor";
    private Guid _createdByUserId = Guid.NewGuid();
    private int _expirationDays = 7;

    public static StaffInvitationBuilder New() => new();

    public StaffInvitationBuilder WithEmail(string email) { _email = email; return this; }
    public StaffInvitationBuilder WithRole(string role) { _role = role; return this; }
    public StaffInvitationBuilder Expired() { _expirationDays = -1; return this; }

    public StaffInvitation Build() =>
        StaffInvitation.Create(_clinicId, _email, _role, _createdByUserId, null, _expirationDays);
}
