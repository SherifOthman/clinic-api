using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Common.Constants;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Radiology order aggregate root - tracks radiology tests from order to results
/// Supports tests ordered during visit OR standalone (patient comes for test only)
/// </summary>
public class RadiologyOrder : BaseEntity
{
    // Private constructor for EF Core
    private RadiologyOrder() { }
    
    public Guid ClinicBranchId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid RadiologyTestId { get; private set; }
    
    // Optional links (flexible!)
    public Guid? MedicalVisitId { get; private set; }  // Null if no visit
    public Guid? OrderedByDoctorId { get; private set; }  // Null if patient self-requested
    
    // Workflow
    public RadiologyStatus Status { get; private set; }
    public DateTime OrderedAt { get; private set; }
    
    // Performing (if done internally)
    public DateTime? PerformedAt { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    
    // Results (images and reports)
    public DateTime? ResultsAvailableAt { get; private set; }
    public Guid? ResultsUploadedByUserId { get; private set; }
    public string? ImageFilePath { get; private set; }
    public string? ReportFilePath { get; private set; }
    public string? Findings { get; private set; }
    
    // Review
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByDoctorId { get; private set; }
    public string? DoctorNotes { get; private set; }
    
    public string? Notes { get; private set; }
    
    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public RadiologyTest RadiologyTest { get; set; } = null!;
    public MedicalVisit? MedicalVisit { get; set; }
    public Doctor? OrderedByDoctor { get; set; }
    
    /// <summary>
    /// Factory: Doctor orders test during visit
    /// </summary>
    public static RadiologyOrder CreateFromVisit(
        Guid clinicBranchId,
        Guid patientId,
        Guid radiologyTestId,
        Guid medicalVisitId,
        Guid orderedByDoctorId,
        string? notes = null)
    {
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (radiologyTestId == Guid.Empty)
            throw new InvalidBusinessOperationException("Radiology test ID is required");
        
        if (medicalVisitId == Guid.Empty)
            throw new InvalidBusinessOperationException("Medical visit ID is required");
        
        if (orderedByDoctorId == Guid.Empty)
            throw new InvalidBusinessOperationException("Ordered by doctor ID is required");
        
        var order = new RadiologyOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            RadiologyTestId = radiologyTestId,
            MedicalVisitId = medicalVisitId,
            OrderedByDoctorId = orderedByDoctorId,
            Status = RadiologyStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };
        
        return order;
    }
    
    /// <summary>
    /// Factory: Patient requests test without visit (standalone)
    /// </summary>
    public static RadiologyOrder CreateStandalone(
        Guid clinicBranchId,
        Guid patientId,
        Guid radiologyTestId,
        string? notes = null)
    {
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (patientId == Guid.Empty)
            throw new InvalidBusinessOperationException("Patient ID is required");
        
        if (radiologyTestId == Guid.Empty)
            throw new InvalidBusinessOperationException("Radiology test ID is required");
        
        var order = new RadiologyOrder
        {
            ClinicBranchId = clinicBranchId,
            PatientId = patientId,
            RadiologyTestId = radiologyTestId,
            MedicalVisitId = null,  // No visit
            OrderedByDoctorId = null,  // No doctor
            Status = RadiologyStatus.Ordered,
            OrderedAt = DateTime.UtcNow,
            Notes = notes
        };
        
        return order;
    }
    
    /// <summary>
    /// Mark test as being performed (internal radiology)
    /// </summary>
    public void MarkAsPerformed(Guid performedByUserId)
    {
        if (Status != RadiologyStatus.Ordered)
            throw new InvalidBusinessOperationException("Test must be in Ordered status");
        
        if (performedByUserId == Guid.Empty)
            throw new InvalidBusinessOperationException("Performed by user ID is required");
        
        Status = RadiologyStatus.InProgress;
        PerformedAt = DateTime.UtcNow;
        PerformedByUserId = performedByUserId;
    }
    
    /// <summary>
    /// Upload results (images and report)
    /// </summary>
    public void UploadResults(
        string? imageFilePath,
        string? reportFilePath,
        string? findings,
        Guid uploadedByUserId)
    {
        if (Status == RadiologyStatus.Cancelled)
            throw new InvalidBusinessOperationException("Cannot upload results for cancelled test");
        
        if (uploadedByUserId == Guid.Empty)
            throw new InvalidBusinessOperationException("Uploaded by user ID is required");
        
        Status = RadiologyStatus.ResultsAvailable;
        ResultsAvailableAt = DateTime.UtcNow;
        ImageFilePath = imageFilePath;
        ReportFilePath = reportFilePath;
        Findings = findings;
        ResultsUploadedByUserId = uploadedByUserId;
    }
    
    /// <summary>
    /// Doctor reviews results
    /// </summary>
    public void Review(Guid reviewedByDoctorId, string? doctorNotes)
    {
        if (Status != RadiologyStatus.ResultsAvailable)
            throw new InvalidBusinessOperationException("Results must be available before review");
        
        if (reviewedByDoctorId == Guid.Empty)
            throw new InvalidBusinessOperationException("Reviewed by doctor ID is required");
        
        Status = RadiologyStatus.Reviewed;
        ReviewedAt = DateTime.UtcNow;
        ReviewedByDoctorId = reviewedByDoctorId;
        DoctorNotes = doctorNotes;
    }
    
    /// <summary>
    /// Cancel the test order
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == RadiologyStatus.Reviewed)
            throw new InvalidBusinessOperationException("Cannot cancel reviewed test");
        
        Status = RadiologyStatus.Cancelled;
        Notes = reason;
    }
}

/// <summary>
/// Status of radiology order
/// </summary>
public enum RadiologyStatus
{
    Ordered,           // Ordered (by doctor or patient)
    InProgress,        // Being performed (internal radiology)
    ResultsAvailable,  // Results uploaded
    Reviewed,          // Doctor reviewed
    Cancelled          // Cancelled
}
