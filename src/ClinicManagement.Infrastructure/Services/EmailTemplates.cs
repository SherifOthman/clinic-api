namespace ClinicManagement.Infrastructure.Services;

public static class EmailTemplates
{
    public static string GetEmailConfirmationTemplate(string firstName, string confirmationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>Confirm Your Email Address</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #2563eb; margin-top: 0;'>Hello {firstName}! ??</h2>
        
        <p>Thank you for registering with our Clinic Management System!</p>
        
        <p>To complete your registration and activate your account, please click the button below to confirm your email address:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{confirmationLink}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Confirm Email Address
            </a>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #f3f4f6; padding: 10px; border-radius: 5px; font-family: monospace; font-size: 12px;'>{confirmationLink}</p>
        
        <div style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b; margin-top: 30px;'>
            <p style='margin: 0;'><strong>? This link will expire in 24 hours</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>If you didn't create an account, you can safely ignore this email.</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>� 2025 Clinic Management System. All rights reserved.</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
    }

    public static string GetPasswordResetTemplate(string firstName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>?? Password Reset Request</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #dc2626; margin-top: 0;'>Hello {firstName}!</h2>
        
        <p>We received a request to reset your password for your Clinic Management System account.</p>
        
        <p>Click the button below to create a new password:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' style='background-color: #dc2626; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Reset My Password
            </a>
        </div>
        
        <div style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b; margin: 20px 0;'>
            <p style='margin: 0;'><strong>? Important:</strong> This link will expire in <strong>1 hour</strong> for security reasons.</p>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #f3f4f6; padding: 10px; border-radius: 5px; font-family: monospace; font-size: 12px;'>{resetLink}</p>
        
        <p style='color: #6b7280; font-size: 14px; margin-top: 30px;'>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>� 2025 Clinic Management System. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    public static string GetStaffInvitationTemplate(string clinicName, string role, string invitedBy, string invitationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>? You're Invited!</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #10b981; margin-top: 0;'>Join {clinicName}</h2>
        
        <p><strong>{invitedBy}</strong> has invited you to join <strong>{clinicName}</strong> as a <strong>{role}</strong>.</p>
        
        <p>Click the button below to accept the invitation and complete your registration:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{invitationLink}' style='background-color: #10b981; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Accept Invitation
            </a>
        </div>
        
        <div style='background-color: #dbeafe; padding: 15px; border-radius: 5px; border-left: 4px solid #3b82f6; margin: 20px 0;'>
            <p style='margin: 0;'><strong>? What's Next?</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>After accepting, you'll create your account and gain access to the clinic management system.</p>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>If the button doesn't work, you can copy and paste this link into your browser:</p>
        <p style='word-break: break-all; background-color: #f3f4f6; padding: 10px; border-radius: 5px; font-family: monospace; font-size: 12px;'>{invitationLink}</p>
        
        <div style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b; margin-top: 30px;'>
            <p style='margin: 0;'><strong>? This invitation will expire in 7 days</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>If you didn't expect this invitation, you can safely ignore this email.</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>� 2025 Clinic Management System. All rights reserved.</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
    }
}
