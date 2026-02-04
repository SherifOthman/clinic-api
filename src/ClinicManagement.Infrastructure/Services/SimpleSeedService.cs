using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public interface ISimpleSeedService
{
    Task SeedBasicDataAsync();
}

public class SimpleSeedService : ISimpleSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SimpleSeedService> _logger;

    public SimpleSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<SimpleSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedBasicDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting basic data seeding...");

            // Check if data already exists
            if (await _context.Specializations.AnyAsync())
            {
                _logger.LogInformation("Seed data already exists. Skipping seeding.");
                return;
            }

            await SeedSpecializationsAsync();
            await SeedMeasurementAttributesAsync();
            await SeedChronicDiseasesAsync();
            await SeedAppointmentTypesAsync();
            await SeedSubscriptionPlansAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Basic data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during basic data seeding");
            throw;
        }
    }

    private async Task SeedSpecializationsAsync()
    {
        _logger.LogInformation("Seeding specializations...");

        var specializations = new[]
        {
            new Specialization { Id = Guid.NewGuid(), Name = "General Medicine - الطب العام", Description = "General medical practice", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Cardiology - أمراض القلب", Description = "Heart and cardiovascular diseases", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Dermatology - الأمراض الجلدية", Description = "Skin diseases and conditions", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Pediatrics - طب الأطفال", Description = "Medical care for children", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Orthopedics - جراحة العظام", Description = "Bone and joint surgery", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Gynecology - أمراض النساء والتوليد", Description = "Women's health and obstetrics", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Neurology - الأمراض العصبية", Description = "Nervous system disorders", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Ophthalmology - طب العيون", Description = "Eye diseases and vision care", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "ENT - الأنف والأذن والحنجرة", Description = "Ear, nose, and throat conditions", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Psychiatry - الطب النفسي", Description = "Mental health and psychiatric disorders", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Urology - المسالك البولية", Description = "Urinary system and male reproductive health", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Gastroenterology - أمراض الجهاز الهضمي", Description = "Digestive system disorders", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Endocrinology - الغدد الصماء", Description = "Hormonal and endocrine disorders", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Pulmonology - أمراض الصدر", Description = "Lung and respiratory diseases", IsActive = true },
            new Specialization { Id = Guid.NewGuid(), Name = "Rheumatology - أمراض الروماتيزم", Description = "Joint and autoimmune diseases", IsActive = true }
        };

        await _context.Specializations.AddRangeAsync(specializations);
        _logger.LogInformation("Added {Count} specializations", specializations.Length);
    }

    private async Task SeedMeasurementAttributesAsync()
    {
        _logger.LogInformation("Seeding measurement attributes...");

        var measurements = new[]
        {
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Blood Pressure Systolic - ضغط الدم الانقباضي", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Blood Pressure Diastolic - ضغط الدم الانبساطي", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Heart Rate - معدل ضربات القلب", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Temperature - درجة الحرارة", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Weight - الوزن", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Height - الطول", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "BMI - مؤشر كتلة الجسم", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Blood Sugar - سكر الدم", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Oxygen Saturation - تشبع الأكسجين", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Respiratory Rate - معدل التنفس", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Pain Scale - مقياس الألم", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Vision Test - فحص النظر", DataType = MeasurementDataType.Text },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Hearing Test - فحص السمع", DataType = MeasurementDataType.Text },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "Cholesterol Level - مستوى الكوليسترول", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { Id = Guid.NewGuid(), Name = "HbA1c - الهيموجلوبين السكري", DataType = MeasurementDataType.Decimal }
        };

        await _context.MeasurementAttributes.AddRangeAsync(measurements);
        _logger.LogInformation("Added {Count} measurement attributes", measurements.Length);
    }

    private async Task SeedChronicDiseasesAsync()
    {
        _logger.LogInformation("Seeding chronic diseases...");

        var diseases = new[]
        {
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Diabetes Type 1", NameAr = "السكري النوع الأول" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Diabetes Type 2", NameAr = "السكري النوع الثاني" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Hypertension", NameAr = "ارتفاع ضغط الدم" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Asthma", NameAr = "الربو" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Heart Disease", NameAr = "أمراض القلب" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Arthritis", NameAr = "التهاب المفاصل" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Chronic Kidney Disease", NameAr = "أمراض الكلى المزمنة" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "COPD", NameAr = "مرض الانسداد الرئوي المزمن" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Thyroid Disorders", NameAr = "اضطرابات الغدة الدرقية" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Depression", NameAr = "الاكتئاب" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Anxiety Disorders", NameAr = "اضطرابات القلق" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Epilepsy", NameAr = "الصرع" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Migraine", NameAr = "الصداع النصفي" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Osteoporosis", NameAr = "هشاشة العظام" },
            new ChronicDisease { Id = Guid.NewGuid(), NameEn = "Fibromyalgia", NameAr = "الألم العضلي الليفي" }
        };

        await _context.ChronicDiseases.AddRangeAsync(diseases);
        _logger.LogInformation("Added {Count} chronic diseases", diseases.Length);
    }

    private async Task SeedAppointmentTypesAsync()
    {
        _logger.LogInformation("Seeding appointment types...");

        var appointmentTypes = new[]
        {
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Consultation", NameAr = "استشارة", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Follow-up", NameAr = "متابعة", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Emergency", NameAr = "طوارئ", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Surgery", NameAr = "جراحة", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Checkup", NameAr = "فحص دوري", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Vaccination", NameAr = "تطعيم", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Lab Results", NameAr = "نتائج المختبر", IsActive = true },
            new AppointmentType { Id = Guid.NewGuid(), NameEn = "Physical Therapy", NameAr = "علاج طبيعي", IsActive = true }
        };

        await _context.AppointmentTypes.AddRangeAsync(appointmentTypes);
        _logger.LogInformation("Added {Count} appointment types", appointmentTypes.Length);
    }

    private async Task SeedSubscriptionPlansAsync()
    {
        _logger.LogInformation("Seeding subscription plans...");

        var subscriptionPlans = new[]
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Free Plan",
                Description = "Basic features for small clinics",
                MonthlyFee = 0,
                YearlyFee = 0,
                SetupFee = 0,
                MaxBranches = 1,
                MaxStaff = 2,
                MaxPatientsPerMonth = 100,
                MaxAppointmentsPerMonth = 50,
                MaxInvoicesPerMonth = 50,
                StorageLimitGB = 1,
                HasInventoryManagement = false,
                HasReporting = true,
                HasAdvancedReporting = false,
                HasApiAccess = false,
                HasMultipleBranches = false,
                HasCustomBranding = false,
                HasPrioritySupport = false,
                HasBackupAndRestore = false,
                HasIntegrations = false,
                IsActive = true,
                IsPopular = false,
                DisplayOrder = 1
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Basic Plan",
                Description = "Essential features for growing clinics",
                MonthlyFee = 99,
                YearlyFee = 990, // 2 months free
                SetupFee = 50,
                MaxBranches = 3,
                MaxStaff = 10,
                MaxPatientsPerMonth = 1000,
                MaxAppointmentsPerMonth = 500,
                MaxInvoicesPerMonth = 500,
                StorageLimitGB = 10,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = true,
                HasApiAccess = false,
                HasMultipleBranches = true,
                HasCustomBranding = false,
                HasPrioritySupport = false,
                HasBackupAndRestore = true,
                HasIntegrations = false,
                IsActive = true,
                IsPopular = true,
                DisplayOrder = 2
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Professional Plan",
                Description = "Advanced features for established clinics",
                MonthlyFee = 199,
                YearlyFee = 1990, // 2 months free
                SetupFee = 0,
                MaxBranches = 10,
                MaxStaff = 50,
                MaxPatientsPerMonth = 10000,
                MaxAppointmentsPerMonth = 2000,
                MaxInvoicesPerMonth = 2000,
                StorageLimitGB = 50,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = true,
                HasApiAccess = true,
                HasMultipleBranches = true,
                HasCustomBranding = true,
                HasPrioritySupport = true,
                HasBackupAndRestore = true,
                HasIntegrations = true,
                IsActive = true,
                IsPopular = false,
                DisplayOrder = 3
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Enterprise Plan",
                Description = "Unlimited features for large healthcare organizations",
                MonthlyFee = 499,
                YearlyFee = 4990, // 2 months free
                SetupFee = 0,
                MaxBranches = 999, // Practically unlimited
                MaxStaff = 999, // Practically unlimited
                MaxPatientsPerMonth = 999999, // Practically unlimited
                MaxAppointmentsPerMonth = 999999, // Practically unlimited
                MaxInvoicesPerMonth = 999999, // Practically unlimited
                StorageLimitGB = 500,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = true,
                HasApiAccess = true,
                HasMultipleBranches = true,
                HasCustomBranding = true,
                HasPrioritySupport = true,
                HasBackupAndRestore = true,
                HasIntegrations = true,
                IsActive = true,
                IsPopular = false,
                DisplayOrder = 4
            }
        };

        await _context.SubscriptionPlans.AddRangeAsync(subscriptionPlans);
        _logger.LogInformation("Added {Count} subscription plans", subscriptionPlans.Length);
    }
}