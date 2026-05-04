namespace ClinicManagement.Application.Common.EmailTemplates;

/// <summary>
/// Email template for usage limit warnings.
/// Lives in Application so both Persistence (jobs) and Infrastructure (email service) can use it.
/// </summary>
public static class UsageLimitEmailTemplate
{
    public static string Build(
        string ownerName, string clinicName,
        string limitLabel, int used, int max, int percent, bool isCritical)
    {
        var color        = isCritical ? "#dc2626" : "#f59e0b";
        var headerText   = isCritical ? "⚠️ Usage Limit Critical" : "📊 Usage Limit Warning";
        var callToAction = isCritical
            ? "You are about to reach your plan limit. Upgrade your plan to avoid service interruption."
            : "You are approaching your plan limit. Consider upgrading before you reach the limit.";

        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: {color}; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>{headerText}</h1>
    </div>
    <div style='background: #fff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <p>Hello {ownerName},</p>
        <p>This is a usage alert for <strong>{clinicName}</strong>.</p>
        <div style='background: #f9fafb; border-left: 4px solid {color}; padding: 16px; border-radius: 4px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 18px; font-weight: bold;'>{limitLabel.ToUpperInvariant()}</p>
            <p style='margin: 8px 0 0 0;'>Used: <strong>{used} / {max}</strong> ({percent}% of monthly limit)</p>
        </div>
        <p>{callToAction}</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='/settings/subscription'
               style='background: {color}; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                View Subscription
            </a>
        </div>
    </div>
    <div style='text-align: center; padding: 20px; color: #6b7280; font-size: 12px;'>
        <p>© 2025 Clinic Management System. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
