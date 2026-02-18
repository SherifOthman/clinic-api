using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents a subscription plan that defines the features and limits for a clinic
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string Description { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    
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
    


    
    /// <summary>
    /// Calculates the yearly discount percentage
    /// </summary>
    public decimal YearlyDiscountPercentage => MonthlyFee > 0 ? Math.Round((1 - (YearlyFee / (MonthlyFee * 12))) * 100, 2) : 0;
    
    /// <summary>
    /// Checks if the plan supports multiple branches
    /// </summary>
    public bool SupportsMultipleBranches => MaxBranches > 1;
    
    /// <summary>
    /// Checks if the plan is a free plan
    /// </summary>
    public bool IsFree => MonthlyFee == 0 && YearlyFee == 0;
    
    /// <summary>
    /// Checks if a clinic can add more branches based on this plan
    /// </summary>
    /// <param name="currentBranchCount">Current number of branches</param>
    /// <returns>True if more branches can be added</returns>
    public bool CanAddMoreBranches(int currentBranchCount)
    {
        return currentBranchCount < MaxBranches;
    }
    
    /// <summary>
    /// Checks if a clinic can add more staff based on this plan
    /// </summary>
    /// <param name="currentStaffCount">Current number of staff</param>
    /// <returns>True if more staff can be added</returns>
    public bool CanAddMoreStaff(int currentStaffCount)
    {
        return currentStaffCount < MaxStaff;
    }
    
    /// <summary>
    /// Checks if the plan has reached the monthly patient limit
    /// </summary>
    /// <param name="currentMonthPatients">Current month patient count</param>
    /// <returns>True if limit is reached</returns>
    public bool HasReachedPatientLimit(int currentMonthPatients)
    {
        return currentMonthPatients >= MaxPatientsPerMonth;
    }
    
    /// <summary>
    /// Checks if the plan has reached the monthly appointment limit
    /// </summary>
    /// <param name="currentMonthAppointments">Current month appointment count</param>
    /// <returns>True if limit is reached</returns>
    public bool HasReachedAppointmentLimit(int currentMonthAppointments)
    {
        return currentMonthAppointments >= MaxAppointmentsPerMonth;
    }
    
    /// <summary>
    /// Checks if the plan has reached the monthly invoice limit
    /// </summary>
    /// <param name="currentMonthInvoices">Current month invoice count</param>
    /// <returns>True if limit is reached</returns>
    public bool HasReachedInvoiceLimit(int currentMonthInvoices)
    {
        return currentMonthInvoices >= MaxInvoicesPerMonth;
    }
}
