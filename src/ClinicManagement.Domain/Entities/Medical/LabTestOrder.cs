using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Events;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Lab test order aggregate root - tracks lab tests from order to results
/// Supports tests ordered during visit OR standalone (patient comes for test only)
/// </summary>
public class LabTestOrder : AggregateRoot
{
    // Private constructor for EF Core
    private LabTestOrder() { }
    
    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid LabTestId { get; private set; }
    
    // Optional links (flexible!)
    public Guid? MedicalVisitId { get; private set; }  // Null if no visit
    public Guid? OrderedByDoctorId { get; private set; }  // Null if patient self-requested
    
    // Workflow
    public LabTestStatus Status { get; private set; }
    public DateTime OrderedAt { get; private set; }
    
    // Performing (if done internally)
    public DateTime? PerformedAt { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    
    // Results
    public DateTime? ResultsAvailableAt { get; private set; }
    public Guid? ResultsUploadedByUserId { get; private set; }
    public string? ResultsFilePath { get; private set; }
    public string? ResultsText { get; private set; }
    public bool? IsAbnormal { get; private set; }
    
    // Review
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByDoctorId { get; private set; }
    public string? DoctorNotes { get; private set; }
    
    public string? Notes { get; private set; }
    
    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public LabTest LabTest { get; set; } = null!;
    public MedicalVisit? MedicalVisit { get; set; }
    public Doctor? OrderedByDoctor { get; set; }
    
    /// <summary>
    /// Factory: Doctor orders test during visit
    /// </summary>
    public static LabTestOrder CreateFromVisit(
        Guid clinicBranchId,
        Guid patientId,
        Guid labTestId,
        Guid medicalVisitId,
        Guid orderedByDoctorId,
        string? notes = null)
    {
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (labTestId == Guid.Empty)
            throw new InvalidBusinessOperationException("Lab test ID is required");
        
        if (medicalVisitId == Guid.Empty)
            throw new InvalidBusinessOperationException("Medical visit ID is required");
        
        if (orderedByDoctorId == Guid.Empty)
            throw new InvalidBusinessOperationException("Ordered by doctor ID is required");
        
        var order = new LabTestOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            LabTestId = labTestId,
            MedicalVisitId = medicalVisitId,
            OrderedByDoctorId = orderedByDoctorId,
            Status = LabTestStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };
        
        order.AddDomainEvent(new LabTestOrderedEvent(
            order.Id,
            patientId,
            labTestId,
            medicalVisitId
        ));
        
        return order;
    }
    
    /// <summary>
    /// Factory: Patient requests test without visit (standalone)
    /// </summary>
    public static LabTestOrder CreateStandalone(
        Guid clinicBranchId,
        Guid patientId,
        Guid labTestId,
        string? notes = null)
    {
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (labTestId == Guid.Empty)
            throw new InvalidBusinessOperationException("Lab test ID is required");
        
        var order = new LabTestOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            LabTestId = labTestId,
            MedicalVisitId = null,  // No visit
            OrderedByDoctorId = null,  // No doctor
            Status = LabTestStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };
        
        order.AddDomainEvent(new LabTestOrderedEvent(
            order.Id,
            patientId,
            labTestId,
            null
        ));
        
        return order;
    }
    
    /// <summary>
    /// Mark test as being performed (internal lab)
    /// </summary>
    public void MarkAsPerformed(Guid performedByUserId)
    {
        if (Status != LabTestStatus.Ordered)
            throw new InvalidBusinessOperationException("Test must be in Ordered status");
        
        if (performedByUserId == Guid.Empty)
            throw new InvalidBusinessOperationException("Performed by user ID is required");
        
        Status = LabTestStatus.InProgress;
        PerformedAt = DateTime.UtcNow;
        PerformedByUserId = performedByUserId;
    }
    
    /// <summary>
    /// Upload results (internal or external lab)
    /// </summary>
    public void UploadResults(
        string? resultsFilePath,
        string? resultsText,
        bool? isAbnormal,
        Guid uploadedByUserId)
    {
        if (Status == LabTestStatus.Cancelled)
            throw new InvalidBusinessOperationException("Cannot upload results for cancelled test");
        
        if (uploadedByUserId == Guid.Empty)
            throw new InvalidBusinessOperationException("Uploaded by user ID is required");
        
        Status = LabTestStatus.ResultsAvailable;
        ResultsAvailableAt = DateTime.UtcNow;
        ResultsFilePath = resultsFilePath;
        ResultsText = resultsText;
        IsAbnormal = isAbnormal;
        ResultsUploadedByUserId = uploadedByUserId;
        
        AddDomainEvent(new LabTestResultsAvailableEvent(
            Id,
            PatientId,
            LabTestId,
            isAbnormal ?? false
        ));
    }
    
    /// <summary>
    /// Doctor reviews results
    /// </summary>
    public void Review(Guid reviewedByDoctorId, string? doctorNotes)
    {
        if (Status != LabTestStatus.ResultsAvailable)
            throw new InvalidBusinessOperationException("Results must be available before review");
        
        if (reviewedByDoctorId == Guid.Empty)
            throw new InvalidBusinessOperationException("Reviewed by doctor ID is required");
        
        Status = LabTestStatus.Reviewed;
        ReviewedAt = DateTime.UtcNow;
        ReviewedByDoctorId = reviewedByDoctorId;
        DoctorNotes = doctorNotes;
        
        AddDomainEvent(new LabTestReviewedEvent(
            Id,
            PatientId,
            reviewedByDoctorId
        ));
    }
    
    /// <summary>
    /// Cancel the test order
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == LabTestStatus.Reviewed)
            throw new InvalidBusinessOperationException("Cannot cancel reviewed test");
        
        Status = LabTestStatus.Cancelled;
        Notes = reason;
    }
}

/// <summary>
/// Status of lab test order
/// </summary>
public enum LabTestStatus
{
    Ordered,           // Ordered (by doctor or patient)
    InProgress,        // Being performed (internal lab)
    ResultsAvailable,  // Results uploaded
    Reviewed,          // Doctor reviewed
    Cancelled          // Cancelled
}
