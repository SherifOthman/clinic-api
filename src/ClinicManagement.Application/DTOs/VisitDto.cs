namespace ClinicManagement.Application.DTOs;

public class VisitDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public AppointmentDto? Appointment { get; set; }
    public List<PrescriptionMedicineDto> PrescriptionMedicines { get; set; } = new();
    public List<VisitAttributeValueDto> VisitAttributeValues { get; set; } = new();
}

public class CreateVisitDto
{
    public int AppointmentId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Description { get; set; }
}

public class MedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
    public string? Description { get; set; }
}

public class CreateMedicineDto
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
    public string? Description { get; set; }
}

public class PrescriptionMedicineDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int MedicineId { get; set; }
    public string? DosageInstructions { get; set; }
    public string? Duration { get; set; }
    public string? Notes { get; set; }
    public MedicineDto? Medicine { get; set; }
}

public class CreatePrescriptionMedicineDto
{
    public int VisitId { get; set; }
    public int MedicineId { get; set; }
    public string? DosageInstructions { get; set; }
    public string? Duration { get; set; }
    public string? Notes { get; set; }
}

public class VisitAttributesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class VisitAttributeValueDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int FieldId { get; set; }
    public string? Value { get; set; }
    public VisitAttributesDto? Field { get; set; }
}
