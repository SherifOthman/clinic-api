namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// HTML email templates.
///
/// All auth flows (email confirmation, password reset) now use OTP codes.
/// Redirect links are no longer used for auth — only staff invitations still use a link
/// because the invitation flow requires the user to land on a specific page.
/// </summary>
public static class EmailTemplates
{
    // ── Shared helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Renders a 6-digit OTP as a row of styled digit boxes.
    /// Each digit gets its own properly-opened and closed &lt;td&gt;.
    /// </summary>
    private static string RenderOtpDigits(string otp, string color, string bg, string border)
    {
        var cells = string.Concat(otp.Select(d =>
            $"<td style='width:44px;height:52px;text-align:center;vertical-align:middle;" +
            $"font-size:28px;font-weight:700;color:{color};background:{bg};" +
            $"border:2px solid {border};border-radius:10px;padding:0;'>{d}</td>"));

        return $"<table role='presentation' cellpadding='0' cellspacing='8' " +
               $"style='margin:28px auto;border-collapse:separate;border-spacing:8px 0;'>" +
               $"<tr>{cells}</tr></table>";
    }

    private static string Footer() =>
        "<div style='text-align:center;padding:18px;color:#9ca3af;font-size:12px;'>" +
        "<p style='margin:0;'>© 2025 ClinicCare. All rights reserved.</p></div>";

    // ── Email confirmation OTP ────────────────────────────────────────────────

    public static string GetEmailConfirmationOtpTemplate(string firstName, string otp)
    {
        var digits = RenderOtpDigits(otp, "#2563eb", "#eff6ff", "#bfdbfe");

        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
  <title>Verify your email</title>
</head>
<body style='font-family:Arial,sans-serif;line-height:1.6;color:#333;max-width:520px;margin:0 auto;padding:20px;background:#f9fafb;'>
  <div style='background:linear-gradient(135deg,#2563eb 0%,#1d4ed8 100%);padding:28px 30px;text-align:center;border-radius:12px 12px 0 0;'>
    <h1 style='color:white;margin:0;font-size:22px;letter-spacing:-0.3px;'>Verify your email address</h1>
  </div>

  <div style='background:#ffffff;padding:32px 30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px;'>
    <p style='margin-top:0;font-size:15px;'>Hi <strong>{firstName}</strong>,</p>
    <p style='font-size:15px;'>Enter this code to confirm your email address.</p>

    {digits}

    <p style='text-align:center;font-size:13px;color:#6b7280;margin-top:0;'>
      This code expires in <strong>5 minutes</strong>.
    </p>

    <div style='background:#fef9c3;padding:14px 16px;border-radius:8px;border-left:4px solid #eab308;margin-top:24px;'>
      <p style='margin:0;font-size:13px;'>
        <strong>Never share this code.</strong>
        If you didn't create an account, you can safely ignore this email.
      </p>
    </div>
  </div>

  {Footer()}
</body>
</html>";
    }

    // ── Password reset OTP ────────────────────────────────────────────────────

    public static string GetPasswordResetOtpTemplate(string firstName, string otp)
    {
        var digits = RenderOtpDigits(otp, "#dc2626", "#fef2f2", "#fecaca");

        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
  <title>Password reset code</title>
</head>
<body style='font-family:Arial,sans-serif;line-height:1.6;color:#333;max-width:520px;margin:0 auto;padding:20px;background:#f9fafb;'>
  <div style='background:linear-gradient(135deg,#dc2626 0%,#b91c1c 100%);padding:28px 30px;text-align:center;border-radius:12px 12px 0 0;'>
    <h1 style='color:white;margin:0;font-size:22px;letter-spacing:-0.3px;'>Password reset code</h1>
  </div>

  <div style='background:#ffffff;padding:32px 30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px;'>
    <p style='margin-top:0;font-size:15px;'>Hi <strong>{firstName}</strong>,</p>
    <p style='font-size:15px;'>Enter this code to reset your password.</p>

    {digits}

    <p style='text-align:center;font-size:13px;color:#6b7280;margin-top:0;'>
      This code expires in <strong>5 minutes</strong>.
    </p>

    <div style='background:#fef9c3;padding:14px 16px;border-radius:8px;border-left:4px solid #eab308;margin-top:24px;'>
      <p style='margin:0;font-size:13px;'>
        <strong>Never share this code.</strong>
        If you didn't request a password reset, ignore this email — your password is unchanged.
      </p>
    </div>
  </div>

  {Footer()}
</body>
</html>";
    }

    // ── Staff invitation (still uses a link — intentional) ────────────────────

    public static string GetStaffInvitationTemplate(
        string clinicName, string role, string invitedBy, string invitationLink)
    {
        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
  <title>You're invited to join {clinicName}</title>
</head>
<body style='font-family:Arial,sans-serif;line-height:1.6;color:#333;max-width:520px;margin:0 auto;padding:20px;background:#f9fafb;'>
  <div style='background:linear-gradient(135deg,#10b981 0%,#059669 100%);padding:28px 30px;text-align:center;border-radius:12px 12px 0 0;'>
    <h1 style='color:white;margin:0;font-size:22px;letter-spacing:-0.3px;'>You're invited!</h1>
  </div>

  <div style='background:#ffffff;padding:32px 30px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px;'>
    <p style='margin-top:0;font-size:15px;'>
      <strong>{invitedBy}</strong> has invited you to join <strong>{clinicName}</strong> as a <strong>{role}</strong>.
    </p>

    <div style='text-align:center;margin:28px 0;'>
      <a href='{invitationLink}'
         style='background:#10b981;color:white;padding:13px 32px;text-decoration:none;
                border-radius:8px;display:inline-block;font-weight:600;font-size:15px;'>
        Accept Invitation
      </a>
    </div>

    <div style='background:#dbeafe;padding:14px 16px;border-radius:8px;border-left:4px solid #3b82f6;margin-bottom:20px;'>
      <p style='margin:0;font-size:13px;'>
        After accepting, you'll set up your account and get access to the clinic management system.
      </p>
    </div>

    <p style='font-size:13px;color:#6b7280;'>
      If the button doesn't work, copy and paste this link into your browser:
    </p>
    <p style='word-break:break-all;background:#f3f4f6;padding:10px;border-radius:6px;
              font-family:monospace;font-size:12px;color:#374151;'>{invitationLink}</p>

    <div style='background:#fef9c3;padding:14px 16px;border-radius:8px;border-left:4px solid #eab308;margin-top:20px;'>
      <p style='margin:0;font-size:13px;'>
        <strong>This invitation expires in 7 days.</strong>
        If you didn't expect this, you can safely ignore it.
      </p>
    </div>
  </div>

  {Footer()}
</body>
</html>";
    }
}
