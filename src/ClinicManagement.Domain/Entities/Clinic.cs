using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Identity;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Owner user of this clinic (ClinicOwner role)
    /// </summary>
    public Guid OwnerUserId { get; set; }
    public ApplicationUser OwnerUser { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}