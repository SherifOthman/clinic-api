using ClinicManagement.Application.Common.Constants;

namespace ClinicManagement.Infrastructure.Templates;

/// <summary>
/// Localized email templates using message codes for internationalization
/// </summary>
public static class LocalizedEmailTemplates
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
        <h1 style='color: white; margin: 0;' data-message-code='{MessageCodes.Email.CONFIRMATION_SUBJECT}'>{{CONFIRMATION_SUBJECT}}</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #2563eb; margin-top: 0;' data-message-code='{MessageCodes.Email.CONFIRMATION_GREETING}'>{{CONFIRMATION_GREETING}} {firstName}! 👋</h2>
        
        <p data-message-code='{MessageCodes.Email.CONFIRMATION_THANK_YOU}'>{{CONFIRMATION_THANK_YOU}}</p>
        
        <p data-message-code='{MessageCodes.Email.CONFIRMATION_INSTRUCTION}'>{{CONFIRMATION_INSTRUCTION}}</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{confirmationLink}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;' data-message-code='{MessageCodes.Email.CONFIRMATION_BUTTON}'>
                {{CONFIRMATION_BUTTON}}
            </a>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;' data-message-code='{MessageCodes.Email.CONFIRMATION_LINK_INSTRUCTION}'>{{CONFIRMATION_LINK_INSTRUCTION}}</p>
        <p style='word-break: break-all; background-color: #f3f4f6; padding: 10px; border-radius: 5px; font-family: monospace; font-size: 12px;'>{confirmationLink}</p>
        
        <div style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b; margin-top: 30px;'>
            <p style='margin: 0;' data-message-code='{MessageCodes.Email.CONFIRMATION_EXPIRY_WARNING}'><strong>⏰ {{CONFIRMATION_EXPIRY_WARNING}}</strong></p>
            <p style='margin: 10px 0 0 0; font-size: 14px;' data-message-code='{MessageCodes.Email.CONFIRMATION_IGNORE_MESSAGE}'>{{CONFIRMATION_IGNORE_MESSAGE}}</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_COPYRIGHT}'>{{EMAIL_FOOTER_COPYRIGHT}}</p>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_AUTOMATED}'>{{EMAIL_FOOTER_AUTOMATED}}</p>
    </div>
</body>
</html>";
    }

    public static string WelcomeEmail(string firstName, string clinicName, DateTime? trialEndDate)
    {
        var trialInfo = trialEndDate.HasValue
            ? $@"<p style='background-color: #fef3c7; padding: 15px; border-radius: 5px; border-left: 4px solid #f59e0b;'>
                    <strong>🎉 {{TRIAL_STARTED}}</strong><br/>
                    {{TRIAL_ENDS}}: <strong>{trialEndDate.Value:MMMM dd, yyyy}</strong>
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
        <h1 style='color: white; margin: 0;' data-message-code='{MessageCodes.Email.WELCOME_SUBJECT}'>{{WELCOME_SUBJECT}}</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #2563eb; margin-top: 0;' data-message-code='{MessageCodes.Email.WELCOME_GREETING}'>{{WELCOME_GREETING}} {firstName}! 👋</h2>
        
        <p data-message-code='{MessageCodes.Email.WELCOME_MESSAGE}'>{{WELCOME_MESSAGE}} <strong>{clinicName}</strong>!</p>
        
        {trialInfo}
        
        <h3 style='color: #2563eb; margin-top: 30px;'>🚀 {{GET_STARTED}}:</h3>
        <ul style='line-height: 2;'>
            <li>✅ {{VERIFY_EMAIL}}</li>
            <li>📋 {{ADD_FIRST_PATIENT}}</li>
            <li>📅 {{SCHEDULE_APPOINTMENTS}}</li>
            <li>💊 {{MANAGE_INVENTORY}}</li>
            <li>👥 {{INVITE_TEAM_MEMBERS}}</li>
        </ul>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{{{FRONTEND_URL}}}}/dashboard' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                {{GO_TO_DASHBOARD}}
            </a>
        </div>
        
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 5px; margin-top: 30px;'>
            <h4 style='margin-top: 0; color: #374151;'>📚 {{NEED_HELP}}?</h4>
            <p style='margin-bottom: 0;'>
                {{CHECK_DOCUMENTATION}} <a href='{{{{FRONTEND_URL}}}}/docs' style='color: #2563eb;'>{{DOCUMENTATION}}</a> {{OR}} 
                <a href='{{{{FRONTEND_URL}}}}/support' style='color: #2563eb;'>{{CONTACT_SUPPORT}}</a>.
            </p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_COPYRIGHT}'>{{EMAIL_FOOTER_COPYRIGHT}}</p>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_AUTOMATED}'>{{EMAIL_FOOTER_AUTOMATED}}</p>
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
        <h1 style='color: white; margin: 0;' data-message-code='{MessageCodes.Email.USAGE_WARNING_SUBJECT}'>{icon} {{USAGE_WARNING_SUBJECT}}</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: {color}; margin-top: 0;'>{{USAGE_THRESHOLD_REACHED}}</h2>
        
        <p>{{HELLO}} <strong>{clinicName}</strong>,</p>
        
        <p data-message-code='{MessageCodes.Email.USAGE_WARNING_MESSAGE}'>{{USAGE_WARNING_MESSAGE}} <strong>{metricType}</strong> {{USAGE_REACHED}} <strong>{percentage}%</strong> {{OF_PLAN_LIMIT}}.</p>
        
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 5px; margin: 20px 0;'>
            <h3 style='margin-top: 0; color: #374151;'>{{CURRENT_USAGE}}:</h3>
            <div style='background-color: #e5e7eb; border-radius: 10px; height: 30px; position: relative; overflow: hidden;'>
                <div style='background: linear-gradient(90deg, {color} 0%, {color} 100%); height: 100%; width: {percentage}%; transition: width 0.3s;'></div>
                <span style='position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); font-weight: bold; color: #1f2937;'>
                    {currentUsage} / {limit} ({percentage}%)
                </span>
            </div>
        </div>
        
        <p>{{AVOID_SERVICE_INTERRUPTION}}</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{{{FRONTEND_URL}}}}/subscription/upgrade' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                {{UPGRADE_PLAN}}
            </a>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_COPYRIGHT}'>{{EMAIL_FOOTER_COPYRIGHT}}</p>
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
        <h1 style='color: white; margin: 0;' data-message-code='{MessageCodes.Email.SUBSCRIPTION_EXPIRING_SUBJECT}'>⏰ {{SUBSCRIPTION_EXPIRING_SUBJECT}}</h1>
    </div>
    
    <div style='background-color: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #f59e0b; margin-top: 0;'>{{ACTION_REQUIRED}}</h2>
        
        <p>{{HELLO}} <strong>{clinicName}</strong>,</p>
        
        <p data-message-code='{MessageCodes.Email.SUBSCRIPTION_EXPIRING_MESSAGE}'>{{SUBSCRIPTION_EXPIRING_MESSAGE}} <strong>{plan}</strong> {{SUBSCRIPTION_WILL_EXPIRE}} <strong>{daysLeft} {{DAYS}}</strong> {{ON}} <strong>{endDate:MMMM dd, yyyy}</strong>.</p>
        
        <div style='background-color: #fef3c7; padding: 20px; border-radius: 5px; border-left: 4px solid #f59e0b; margin: 20px 0;'>
            <p style='margin: 0;'><strong>{{DONT_LOSE_ACCESS}}</strong></p>
            <p style='margin: 10px 0 0 0;'>{{RENEW_TO_CONTINUE}}</p>
        </div>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{{{{FRONTEND_URL}}}}/subscription/renew' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                {{RENEW_SUBSCRIPTION}}
            </a>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>{{QUESTIONS_CONTACT_SUPPORT}}</p>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p data-message-code='{MessageCodes.Email.EMAIL_FOOTER_COPYRIGHT}'>{{EMAIL_FOOTER_COPYRIGHT}}</p>
    </div>
</body>
</html>";
    }
}