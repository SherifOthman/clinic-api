namespace ClinicManagement.Application.Abstractions.Email;

/// <summary>
/// Represents a fully-formed email ready to be sent.
/// Each email type is its own class — adding a new email type means adding
/// a new class, not modifying EmailService (OCP).
/// </summary>
public interface IEmail
{
    string ToEmail  { get; }
    string? ToName  { get; }
    string Subject  { get; }
    string Body     { get; }
    bool   IsHtml   { get; }
}
