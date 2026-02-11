using System;
using System.Collections.Generic;
using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class Clinic : AuditableEntity
{
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Owner user of this clinic (ClinicOwner role)
    /// </summary>
    public Guid OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
