using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction table between MedicalVisit and MeasurementAttribute
/// Stores the actual measurement values for each visit
/// </summary>
public class MedicalVisitMeasurement : BaseEntity
{
    public Guid MedicalVisitId { get; set; }
    public Guid MeasurementAttributeId { get; set; }  
    
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public bool? BooleanValue { get; set; }
    
    public string? Notes { get; set; }

    // Navigation properties
    public MedicalVisit MedicalVisit { get; set; } = null!;
    public MeasurementAttribute MeasurementAttribute { get; set; } = null!;
    
    // Helper methods to get/set values based on data type
    public object? GetValue()
    {
        return MeasurementAttribute?.DataType switch
        {
            Common.Enums.MeasurementDataType.String => StringValue,
            Common.Enums.MeasurementDataType.Integer => IntValue,
            Common.Enums.MeasurementDataType.Decimal => DecimalValue,
            Common.Enums.MeasurementDataType.DateTime => DateTimeValue,
            Common.Enums.MeasurementDataType.Boolean => BooleanValue,
            _ => null
        };
    }
    
    public void SetValue(object? value)
    {
        // Clear all values first
        StringValue = null;
        IntValue = null;
        DecimalValue = null;
        DateTimeValue = null;
        BooleanValue = null;
        
        // Set the appropriate value based on data type
        if (value != null && MeasurementAttribute != null)
        {
            switch (MeasurementAttribute.DataType)
            {
                case Common.Enums.MeasurementDataType.String:
                    StringValue = value.ToString();
                    break;
                case Common.Enums.MeasurementDataType.Integer:
                    IntValue = Convert.ToInt32(value);
                    break;
                case Common.Enums.MeasurementDataType.Decimal:
                    DecimalValue = Convert.ToDecimal(value);
                    break;
                case Common.Enums.MeasurementDataType.DateTime:
                    DateTimeValue = Convert.ToDateTime(value);
                    break;
                case Common.Enums.MeasurementDataType.Boolean:
                    BooleanValue = Convert.ToBoolean(value);
                    break;
            }
        }
    }
}