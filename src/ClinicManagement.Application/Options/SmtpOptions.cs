using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Application.Options;

public class SmtpOptions
{
    [Required(ErrorMessage = "SMTP Host is required")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535")]
    public int Port { get; set; } = 587;

    [Required(ErrorMessage = "SMTP UserName is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "SMTP Password is required")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "From Name is required")]
    public string FromName { get; set; } = string.Empty;

    [Required(ErrorMessage = "From Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string FromEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Frontend URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string FrontendUrl { get; set; } = string.Empty;
}
