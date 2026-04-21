using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores measurement values for a medical visit as a JSON column.
///
/// Replaces the EAV pattern (5 nullable typed columns) with a single
/// ValuesJson column. This avoids pivot queries, is easier to extend,
/// and SQL Server 2022+ supports JSON path indexes for querying.
///
/// Format: { "weight": 75.5, "bloodPressure": "120/80", "temperature": 37.2 }
/// Keys are MeasurementAttribute names; values are always stored as strings.
/// </summary>
public class MedicalVisitMeasurement : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    public Guid MeasurementAttributeId { get; set; }

    /// <summary>
    /// JSON object storing all measurement values for this attribute.
    /// Use ValuesJson to read/write; parse in the application layer.
    /// </summary>
    public string ValuesJson { get; set; } = "{}";

    public string? Notes { get; set; }
}
