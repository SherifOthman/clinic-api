using ClinicManagement.Domain.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.Application.Common.Attributes;

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

public class OwnerOnlyAttribute : RequireRoleAttribute
{
    public OwnerOnlyAttribute() : base(RoleNames.ClinicOwner) { }
}

public class StaffOnlyAttribute : RequireRoleAttribute
{
    public StaffOnlyAttribute() : base(RoleNames.ClinicOwner, RoleNames.Doctor, RoleNames.Receptionist) { }
}

public class MedicalStaffAttribute : RequireRoleAttribute
{
    public MedicalStaffAttribute() : base(RoleNames.ClinicOwner, RoleNames.Doctor) { }
}
