using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Application.Options;

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = string.Empty;
}
