namespace ClinicManagement.Infrastructure.Templates;

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
        <h2 style='color: #2563eb; margin-top: 0;'>Hello {firstName}! 👋</h2>
        
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
            <p style='margin: 0;'><strong>⏰ This link will expire in 24 hours</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;'>If you didn't create an account, you can safely ignore this email.</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
    }

    public static string WelcomeEmail(string firstName, string clinicName, DateTime? trialEndDate)
    {
        var trialInfo = trialEndDate.HasValue
            ? $@"<p style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b;'>
                    <strong>🎉 Your 14-day free trial has started!</strong><br/>
                    Trial ends on: <strong>{trialEndDate.Value:MMMM dd, yyyy}</strong>
                 </p>"
            : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>Welcome to Clinic Management System!</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #2563eb; margin-top: 0;'>Hello {firstName}! 👋</h2>
        
        <p>Thank you for choosing our Clinic Management System for <strong>{clinicName}</strong>!</p>
        
        {trialInfo}
        
        <h3 style='color: #2563eb; margin-top: 30px;'>🚀 Get Started:</h3>
        <ul style='line-height: 2;'>
            <li>✅ Verify your email address</li>
            <li>📋 Add your first patient</li>
            <li>📅 Schedule appointments</li>
            <li>💊 Manage inventory</li>
            <li>👥 Invite team members</li>
        </ul>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{FRONTEND_URL}}/dashboard' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Go to Dashboard
            </a>
        </div>
        
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 5px; margin-top: 30px;'>
            <h4 style='margin-top: 0; color: #374151;'>📚 Need Help?</h4>
            <p style='margin-bottom: 0;'>
                Check out our <a href='{{FRONTEND_URL}}/docs' style='color: #2563eb;'>documentation</a> or 
                <a href='{{FRONTEND_URL}}/support' style='color: #2563eb;'>contact support</a>.
            </p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
        <p>This is an automated message, please do not reply to this email.</p>
    </div>
</body>
</html>";
    }

    public static string UsageWarning(string clinicName, string metricType, int percentage, int currentUsage, int limit)
    {
        var severity = percentage >= 90 ? "critical" : "warning";
        var color = percentage >= 90 ? "#dc2626" : "#f59e0b";
        var icon = percentage >= 90 ? "🚨" : "⚠️";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: {color}; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>{icon} Usage Alert</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: {color}; margin-top: 0;'>Usage Threshold Reached</h2>
        
        <p>Hello <strong>{clinicName}</strong>,</p>
        
        <p>Your <strong>{metricType}</strong> usage has reached <strong>{percentage}%</strong> of your plan limit.</p>
        
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 5px; margin: 20px 0;'>
            <h3 style='margin-top: 0; color: #374151;'>Current Usage:</h3>
            <div style='background-color: #e5e7eb; border-radius: 10px; height: 30px; position: relative; overflow: hidden;'>
                <div style='background: linear-gradient(90deg, {color} 0%, {color} 100%); height: 100%; width: {percentage}%; transition: width 0.3s;'></div>
                <span style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); font-weight: bold; color: #1f2937;'>
                    {currentUsage} / {limit} ({percentage}%)
                </span>
            </div>
        </div>
        
        <p>To avoid service interruption, please consider upgrading your subscription plan.</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{FRONTEND_URL}}/subscription/upgrade' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Upgrade Plan
            </a>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    public static string SubscriptionExpiring(string clinicName, string plan, DateTime endDate, int daysLeft)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f59e0b; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>⏰ Subscription Expiring Soon</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #f59e0b; margin-top: 0;'>Action Required</h2>
        
        <p>Hello <strong>{clinicName}</strong>,</p>
        
        <p>Your <strong>{plan}</strong> subscription will expire in <strong>{daysLeft} days</strong> on <strong>{endDate:MMMM dd, yyyy}</strong>.</p>
        
        <div style='background-color: #fef3c7; padding: 20px; border-radius: 5px; border-left: 4px solid #f59e0b; margin: 20px 0;'>
            <p style='margin: 0;'><strong>Don't lose access to your data!</strong></p>
            <p style='margin: 10px 0 0 0;'>Renew your subscription to continue using all features.</p>
        </div>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{FRONTEND_URL}}/subscription/renew' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Renew Subscription
            </a>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>If you have any questions, please contact our support team.</p>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
