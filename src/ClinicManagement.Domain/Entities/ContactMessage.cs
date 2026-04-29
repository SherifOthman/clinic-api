using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string Email     { get; set; } = null!;
    public string? Phone    { get; set; }
    public string? Company  { get; set; }
    public string Subject   { get; set; } = null!;
    public string Message   { get; set; } = null!;
    public bool IsRead      { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
