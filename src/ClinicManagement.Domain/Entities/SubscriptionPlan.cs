using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = null!;
    public decimal MonthlyFee { get; set; }
    public int MaxBranches { get; set; }
    public int MaxStaff { get; set; }
}