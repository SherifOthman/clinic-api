using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a subscription plan that defines the features and limits for a clinic
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DescriptionAr { get; set; } = null!;
    
    // Pricing
    public decimal MonthlyFee { get; set; }
    public decimal YearlyFee { get; set; }
    public decimal SetupFee { get; set; }
    
    // Limits
    public int MaxBranches { get; set; }
    public int MaxStaff { get; set; }
    public int MaxPatientsPerMonth { get; set; }
    public int MaxAppointmentsPerMonth { get; set; }
    public int MaxInvoicesPerMonth { get; set; }
    
    // Storage limits (in GB)
    public int StorageLimitGB { get; set; }
    
    // Features
    public bool HasInventoryManagement { get; set; }
    public bool HasReporting { get; set; }
    public bool HasAdvancedReporting { get; set; }
    public bool HasApiAccess { get; set; }
    public bool HasMultipleBranches { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasBackupAndRestore { get; set; }
    public bool HasIntegrations { get; set; }
    
    // Plan status
    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; } = false;
    
    // Display order for UI
    public int DisplayOrder { get; set; }
    
    public int Version { get; set; } = 1;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Business logic - calculated properties
    
    public decimal YearlyDiscountPercentage => MonthlyFee > 0 ? Math.Round((1 - (YearlyFee / (MonthlyFee * 12))) * 100, 2) : 0;

    public bool SupportsMultipleBranches => MaxBranches > 1;
    
    public bool IsFree => MonthlyFee == 0 && YearlyFee == 0;
    
    public bool CanAddMoreBranches(int currentBranchCount)
    {
        return currentBranchCount < MaxBranches;
    }

    public bool CanAddMoreStaff(int currentStaffCount)
    {
        return currentStaffCount < MaxStaff;
    }
    public bool HasReachedPatientLimit(int currentMonthPatients)
    {
        return currentMonthPatients >= MaxPatientsPerMonth;
    }
    public bool HasReachedAppointmentLimit(int currentMonthAppointments)
    {
        return currentMonthAppointments >= MaxAppointmentsPerMonth;
    }
    
    public bool HasReachedInvoiceLimit(int currentMonthInvoices)
    {
        return currentMonthInvoices >= MaxInvoicesPerMonth;
    }
}
