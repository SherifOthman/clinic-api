namespace ClinicManagement.Application.Common.Templates;

public static class EmailTemplates
{
    public static string EmailConfirmation(string firstName, string confirmationLink) => $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>Welcome to Clinic Management System!</h2>
            <p>Hello {firstName},</p>
            <p>Thank you for registering with us. To complete your registration and start using our platform, please confirm your email address by clicking the button below:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{confirmationLink}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirm Email Address</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #6b7280;'>{confirmationLink}</p>
            <p style='margin-top: 30px; color: #6b7280; font-size: 14px;'>If you didn't create an account with us, please ignore this email.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
            <p style='color: #9ca3af; font-size: 12px;'>This is an automated message, please do not reply to this email.</p>
        </div>";

    public static string PasswordReset(string firstName, string resetLink) => $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
                .button {{ display: inline-block; padding: 14px 32px; background: #667eea; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
                .button:hover {{ background: #5568d3; }}
                .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
                .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px; margin: 20px 0; border-radius: 4px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1 style='margin: 0;'>🔐 Password Reset Request</h1>
                </div>
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
                    <p>We received a request to reset your password for your ClinicFlow account. Click the button below to create a new password:</p>
                    <div style='text-align: center;'>
                        <a href='{resetLink}' class='button' style='color: white !important;'>Reset My Password</a>
                    </div>
                    <div class='warning'>
                        <strong>⏰ Important:</strong> This link will expire in <strong>1 hour</strong> for security reasons.
                    </div>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>
                    <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                    <p style='color: #6b7280; font-size: 14px;'>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                </div>
                <div class='footer'>
                    <p>© 2025 ClinicFlow. All rights reserved.</p>
                    <p>Streamline your healthcare practice with confidence.</p>
                </div>
            </div>
        </body>
        </html>";

    public static string StaffInvitation(string firstName, string clinicName, string inviterName, string role, string invitationLink, DateTime expiryDate) => $@"
        <html>
        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h2 style='margin: 0;'>You've been invited to join {clinicName}!</h2>
            </div>
            <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <p>Hi {firstName},</p>
                <p>{inviterName} has invited you to join <strong>{clinicName}</strong> as a <strong>{role}</strong>.</p>
                <p>To accept this invitation and complete your registration, please click the button below:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{invitationLink}' style='background-color: #4CAF50; color: white; padding: 14px 28px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>
                        Accept Invitation
                    </a>
                </div>
                <p>Or copy and paste this link into your browser:</p>
                <p style='color: #666; word-break: break-all;'>{invitationLink}</p>
                <div style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b; margin: 20px 0;'>
                    <p style='margin: 0; color: #92400e;'><strong>⏰ Important:</strong> This invitation will expire on <strong>{expiryDate:MMMM dd, yyyy}</strong> at <strong>{expiryDate:HH:mm}</strong> UTC.</p>
                </div>
                <p style='color: #6b7280; font-size: 14px;'>If you didn't expect this invitation, you can safely ignore this email.</p>
            </div>
            <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
                <p>© 2025 ClinicFlow. All rights reserved.</p>
            </div>
        </body>
        </html>";
}