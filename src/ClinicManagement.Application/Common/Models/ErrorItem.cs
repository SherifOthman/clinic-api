namespace ClinicManagement.Application.Common.Models;

public class ErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    
    public ErrorItem() { }
    
    public ErrorItem(string field, string code)
    {
        Field = field;
        Code = code;
    }
}
