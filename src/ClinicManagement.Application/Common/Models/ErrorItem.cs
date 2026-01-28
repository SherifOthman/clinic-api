namespace ClinicManagement.Application.Common.Models;

public class ErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public ErrorItem() { }
    
    public ErrorItem(string field, string message)
    {
        Field = field;
        Message = message;
    }
}
