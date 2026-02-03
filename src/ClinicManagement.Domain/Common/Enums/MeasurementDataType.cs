namespace ClinicManagement.Domain.Common.Enums;

public enum MeasurementDataType : byte
{
    String = 0,
    Number = 1,
    Boolean = 2,
    Date = 3,
    Structured = 4 // For complex measurements like Blood Pressure
}