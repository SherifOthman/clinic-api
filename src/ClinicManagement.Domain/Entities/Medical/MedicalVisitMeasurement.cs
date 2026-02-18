using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

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
    public DateTime? DateTimeValue { get; set; }
    public bool? BooleanValue { get; set; }
    
    public string? Notes { get; set; }


    public MedicalVisit MedicalVisit { get; set; } = null!;
    public MeasurementAttribute MeasurementAttribute { get; set; } = null!;
    

    public object? GetValue()
    {
        return MeasurementAttribute?.DataType switch
        {
            MeasurementDataType.Text => StringValue,
            MeasurementDataType.Integer => IntValue,
            MeasurementDataType.Decimal => DecimalValue,
            MeasurementDataType.Boolean => BooleanValue,
            _ => null
        };
    }
    
    public void SetValue(object? value)
    {
        // Clear all values first
        StringValue = null;
        IntValue = null;
        DecimalValue = null;
        BooleanValue = null;
        

        if (value != null && MeasurementAttribute != null)
        {
            switch (MeasurementAttribute.DataType)
            {
                case MeasurementDataType.Text:
                    StringValue = value.ToString();
                    break;
                case MeasurementDataType.Integer:
                    IntValue = Convert.ToInt32(value);
                    break;
                case MeasurementDataType.Decimal:
                    DecimalValue = Convert.ToDecimal(value);
                    break;
                case MeasurementDataType.Boolean:
                    BooleanValue = Convert.ToBoolean(value);
                    break;
            }
        }
    }
}
