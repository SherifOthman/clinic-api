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
    public User Owner { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    /// <summary>
    /// Whether the clinic setup/onboarding has been completed
    /// </summary>
    public bool OnboardingCompleted { get; set; }
    
    /// <summary>
    /// Date when onboarding was completed
    /// </summary>
    public DateTime? OnboardingCompletedDate { get; set; }
    
    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
