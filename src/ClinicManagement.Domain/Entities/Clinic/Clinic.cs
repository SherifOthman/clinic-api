using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public Guid OwnerUserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTime? OnboardingCompletedDate { get; set; }
}
