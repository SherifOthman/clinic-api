using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class ComprehensiveSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly CodeGeneratorService _codeGenerator;
    private readonly ILogger<ComprehensiveSeedService> _logger;

    // Store created entities for relationships
    private List<Specialization> _specializations = new();
    private List<MeasurementAttribute> _measurementAttributes = new();
    private List<ChronicDisease> _chronicDiseases = new();
    private List<AppointmentType> _appointmentTypes = new();
    private List<SubscriptionPlan> _subscriptionPlans = new();

    public ComprehensiveSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        CodeGeneratorService codeGenerator,
        ILogger<ComprehensiveSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task SeedReferenceDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting reference data seeding...");

            // Always seed roles and superadmin (they check internally if they exist)
            await SeedRolesAsync();
            await SeedSuperAdminAsync();

            // Check if other reference data already exists
            if (await _context.Specializations.AnyAsync())
            {
                _logger.LogInformation("Reference data already exists. Skipping reference data seeding.");
                return;
            }

            // Seed reference data only - in order of dependencies
            await SeedSpecializationsAsync();
            await SeedMeasurementAttributesAsync();
            await SeedChronicDiseasesAsync();
            await SeedAppointmentTypesAsync();
            await SeedSubscriptionPlansAsync();
            await SeedSpecializationMeasurementAttributesAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Reference data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reference data seeding");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        var roles = new[]
        {
            "SuperAdmin",
            "ClinicOwner", 
            "Doctor",
            "Receptionist"
        };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
            }
        }

        _logger.LogInformation("Added {Count} roles", roles.Length);
    }

    private async Task SeedSuperAdminAsync()
    {
        _logger.LogInformation("Seeding super admin user...");

        // Check if superadmin already exists
        try
        {
            var existingAdmin = await _userManager.FindByEmailAsync("superadmin@clinic.com");
            if (existingAdmin != null)
            {
                _logger.LogInformation("Super admin user already exists. Skipping.");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking for existing superadmin, will attempt to create");
        }

        // Create SuperAdmin user (no clinic association, no Staff record)
        var superAdmin = new User
        {
            UserName = "superadmin@clinic.com",
            Email = "superadmin@clinic.com",
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            _logger.LogInformation("Created super admin user");
        }
        else
        {
            _logger.LogWarning("Failed to create super admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedSpecializationsAsync()
    {
        _logger.LogInformation("Seeding specializations...");

        var specializations = new[]
        {
            new Specialization { NameEn = "General Medicine", NameAr = "الطب العام", DescriptionEn = "General medical practice and primary care", DescriptionAr = "الممارسة الطبية العامة والرعاية الأولية", IsActive = true },
            new Specialization { NameEn = "Internal Medicine", NameAr = "الطب الباطني", DescriptionEn = "Diagnosis and treatment of adult diseases", DescriptionAr = "تشخيص وعلاج أمراض البالغين", IsActive = true },
            new Specialization { NameEn = "Cardiology", NameAr = "أمراض القلب", DescriptionEn = "Heart and cardiovascular diseases", DescriptionAr = "أمراض القلب والأوعية الدموية", IsActive = true },
            new Specialization { NameEn = "Dermatology", NameAr = "الأمراض الجلدية", DescriptionEn = "Skin diseases and conditions", DescriptionAr = "الأمراض والحالات الجلدية", IsActive = true },
            new Specialization { NameEn = "Pediatrics", NameAr = "طب الأطفال", DescriptionEn = "Medical care for infants, children and adolescents", DescriptionAr = "الرعاية الطبية للرضع والأطفال والمراهقين", IsActive = true },
            new Specialization { NameEn = "Orthopedics", NameAr = "جراحة العظام", DescriptionEn = "Bone, joint and musculoskeletal surgery", DescriptionAr = "جراحة العظام والمفاصل والجهاز العضلي الهيكلي", IsActive = true },
            new Specialization { NameEn = "Gynecology & Obstetrics", NameAr = "أمراض النساء والتوليد", DescriptionEn = "Women's health, pregnancy and childbirth", DescriptionAr = "صحة المرأة والحمل والولادة", IsActive = true },
            new Specialization { NameEn = "Neurology", NameAr = "الأمراض العصبية", DescriptionEn = "Nervous system disorders", DescriptionAr = "اضطرابات الجهاز العصبي", IsActive = true },
            new Specialization { NameEn = "Ophthalmology", NameAr = "طب العيون", DescriptionEn = "Eye diseases and vision care", DescriptionAr = "أمراض العيون ورعاية البصر", IsActive = true },
            new Specialization { NameEn = "ENT", NameAr = "الأنف والأذن والحنجرة", DescriptionEn = "Ear, nose, and throat conditions", DescriptionAr = "حالات الأذن والأنف والحنجرة", IsActive = true }
        };

        await _context.Specializations.AddRangeAsync(specializations);
        _specializations.AddRange(specializations);
        _logger.LogInformation("Added {Count} specializations", specializations.Length);
    }

    private async Task SeedMeasurementAttributesAsync()
    {
        _logger.LogInformation("Seeding measurement attributes...");

        var measurements = new[]
        {
            new MeasurementAttribute { NameEn = "Blood Pressure Systolic", NameAr = "ضغط الدم الانقباضي", DescriptionEn = "Systolic blood pressure measurement", DescriptionAr = "قياس ضغط الدم الانقباضي", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { NameEn = "Blood Pressure Diastolic", NameAr = "ضغط الدم الانبساطي", DescriptionEn = "Diastolic blood pressure measurement", DescriptionAr = "قياس ضغط الدم الانبساطي", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { NameEn = "Heart Rate", NameAr = "معدل ضربات القلب", DescriptionEn = "Heart beats per minute", DescriptionAr = "ضربات القلب في الدقيقة", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { NameEn = "Body Temperature", NameAr = "درجة حرارة الجسم", DescriptionEn = "Body temperature in Celsius", DescriptionAr = "درجة حرارة الجسم بالمئوية", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { NameEn = "Weight", NameAr = "الوزن", DescriptionEn = "Body weight in kilograms", DescriptionAr = "وزن الجسم بالكيلوجرام", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { NameEn = "Height", NameAr = "الطول", DescriptionEn = "Body height in centimeters", DescriptionAr = "طول الجسم بالسنتيمتر", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { NameEn = "BMI", NameAr = "مؤشر كتلة الجسم", DescriptionEn = "Body Mass Index", DescriptionAr = "مؤشر كتلة الجسم", DataType = MeasurementDataType.Decimal },
            new MeasurementAttribute { NameEn = "Blood Sugar", NameAr = "سكر الدم", DescriptionEn = "Blood glucose level", DescriptionAr = "مستوى الجلوكوز في الدم", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { NameEn = "Oxygen Saturation", NameAr = "تشبع الأكسجين", DescriptionEn = "Blood oxygen saturation percentage", DescriptionAr = "نسبة تشبع الأكسجين في الدم", DataType = MeasurementDataType.Integer },
            new MeasurementAttribute { NameEn = "Pain Scale", NameAr = "مقياس الألم", DescriptionEn = "Pain level from 0-10", DescriptionAr = "مستوى الألم من 0-10", DataType = MeasurementDataType.Integer }
        };

        await _context.MeasurementAttributes.AddRangeAsync(measurements);
        _measurementAttributes.AddRange(measurements);
        _logger.LogInformation("Added {Count} measurement attributes", measurements.Length);
    }

    private async Task SeedChronicDiseasesAsync()
    {
        _logger.LogInformation("Seeding chronic diseases...");

        var diseases = new[]
        {
            new ChronicDisease { NameEn = "Type 1 Diabetes", NameAr = "السكري النوع الأول", DescriptionEn = "Autoimmune destruction of insulin-producing cells", DescriptionAr = "تدمير مناعي ذاتي للخلايا المنتجة للأنسولين" },
            new ChronicDisease { NameEn = "Type 2 Diabetes", NameAr = "السكري النوع الثاني", DescriptionEn = "Insulin resistance and relative insulin deficiency", DescriptionAr = "مقاومة الأنسولين ونقص نسبي في الأنسولين" },
            new ChronicDisease { NameEn = "Hypertension", NameAr = "ارتفاع ضغط الدم", DescriptionEn = "High blood pressure", DescriptionAr = "ارتفاع ضغط الدم" },
            new ChronicDisease { NameEn = "Asthma", NameAr = "الربو", DescriptionEn = "Chronic inflammatory airway disease", DescriptionAr = "مرض التهابي مزمن في المجاري الهوائية" },
            new ChronicDisease { NameEn = "Coronary Artery Disease", NameAr = "مرض الشريان التاجي", DescriptionEn = "Narrowing of coronary arteries", DescriptionAr = "تضيق الشرايين التاجية" },
            new ChronicDisease { NameEn = "Rheumatoid Arthritis", NameAr = "التهاب المفاصل الروماتويدي", DescriptionEn = "Autoimmune joint inflammation", DescriptionAr = "التهاب المفاصل المناعي الذاتي" },
            new ChronicDisease { NameEn = "Osteoarthritis", NameAr = "التهاب المفاصل التنكسي", DescriptionEn = "Degenerative joint disease", DescriptionAr = "مرض المفاصل التنكسي" },
            new ChronicDisease { NameEn = "Chronic Kidney Disease", NameAr = "أمراض الكلى المزمنة", DescriptionEn = "Progressive loss of kidney function", DescriptionAr = "فقدان تدريجي لوظائف الكلى" },
            new ChronicDisease { NameEn = "COPD", NameAr = "مرض الانسداد الرئوي المزمن", DescriptionEn = "Chronic obstructive pulmonary disease", DescriptionAr = "مرض الانسداد الرئوي المزمن" },
            new ChronicDisease { NameEn = "Hypothyroidism", NameAr = "قصور الغدة الدرقية", DescriptionEn = "Underactive thyroid gland", DescriptionAr = "خمول الغدة الدرقية" }
        };

        await _context.ChronicDiseases.AddRangeAsync(diseases);
        _chronicDiseases.AddRange(diseases);
        _logger.LogInformation("Added {Count} chronic diseases", diseases.Length);
    }

    private async Task SeedAppointmentTypesAsync()
    {
        _logger.LogInformation("Seeding appointment types...");

        var appointmentTypes = new[]
        {
            new AppointmentType { NameEn = "Initial Consultation", NameAr = "استشارة أولية", IsActive = true },
            new AppointmentType { NameEn = "Follow-up Visit", NameAr = "زيارة متابعة", IsActive = true },
            new AppointmentType { NameEn = "Emergency Visit", NameAr = "زيارة طوارئ", IsActive = true },
            new AppointmentType { NameEn = "Annual Checkup", NameAr = "فحص سنوي", IsActive = true },
            new AppointmentType { NameEn = "Vaccination", NameAr = "تطعيم", IsActive = true },
            new AppointmentType { NameEn = "Physical Therapy", NameAr = "علاج طبيعي", IsActive = true },
            new AppointmentType { NameEn = "Chronic Disease Management", NameAr = "إدارة الأمراض المزمنة", IsActive = true },
            new AppointmentType { NameEn = "Preventive Care", NameAr = "الرعاية الوقائية", IsActive = true }
        };

        await _context.AppointmentTypes.AddRangeAsync(appointmentTypes);
        _appointmentTypes.AddRange(appointmentTypes);
        _logger.LogInformation("Added {Count} appointment types", appointmentTypes.Length);
    }

    private async Task SeedSubscriptionPlansAsync()
    {
        _logger.LogInformation("Seeding subscription plans...");

        var subscriptionPlans = new[]
        {
            // Starter Plan - For individual practitioners
            new SubscriptionPlan
            {
                Name = "Starter Plan",
                NameAr = "الباقة التمهيدية",
                Description = "Perfect for solo practitioners starting out",
                DescriptionAr = "مثالية للممارسين الأفراد المبتدئين",
                MonthlyFee = 49,
                YearlyFee = 490,
                SetupFee = 0,
                MaxBranches = 1,
                MaxStaff = 3,
                MaxPatientsPerMonth = 200,
                MaxAppointmentsPerMonth = 150,
                MaxInvoicesPerMonth = 150,
                StorageLimitGB = 5,
                HasInventoryManagement = false,
                HasReporting = true,
                HasAdvancedReporting = false,
                HasApiAccess = false,
                HasMultipleBranches = false,
                HasCustomBranding = false,
                HasPrioritySupport = false,
                HasBackupAndRestore = true,
                HasIntegrations = false,
                IsActive = true,
                IsPopular = false,
                DisplayOrder = 1
            },
            
            // Basic Plan - For small clinics (Most Popular)
            new SubscriptionPlan
            {
                Name = "Basic Plan",
                NameAr = "الباقة الأساسية",
                Description = "Essential features for small clinics",
                DescriptionAr = "الميزات الأساسية للعيادات الصغيرة",
                MonthlyFee = 99,
                YearlyFee = 990,
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
                IsPopular = true, // Marked as popular
                DisplayOrder = 2
            },
            
            // Professional Plan - For established clinics
            new SubscriptionPlan
            {
                Name = "Professional Plan",
                NameAr = "الباقة الاحترافية",
                Description = "Advanced features for established clinics",
                DescriptionAr = "ميزات متقدمة للعيادات المتقدمة",
                MonthlyFee = 199,
                YearlyFee = 1990,
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
            
            // Enterprise Plan - For large healthcare organizations
            new SubscriptionPlan
            {
                Name = "Enterprise Plan",
                NameAr = "باقة المؤسسات",
                Description = "Complete solution for large healthcare organizations",
                DescriptionAr = "حل شامل للمؤسسات الصحية الكبيرة",
                MonthlyFee = 399,
                YearlyFee = 3990,
                SetupFee = 0,
                MaxBranches = -1, // Unlimited
                MaxStaff = -1, // Unlimited
                MaxPatientsPerMonth = -1, // Unlimited
                MaxAppointmentsPerMonth = -1, // Unlimited
                MaxInvoicesPerMonth = -1, // Unlimited
                StorageLimitGB = 200,
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
        _subscriptionPlans.AddRange(subscriptionPlans);
        _logger.LogInformation("Added {Count} subscription plans", subscriptionPlans.Length);
    }

    private async Task SeedSpecializationMeasurementAttributesAsync()
    {
        _logger.LogInformation("Seeding specialization measurement attributes...");

        if (!_specializations.Any() || !_measurementAttributes.Any()) return;

        var specializationMeasurements = new List<SpecializationMeasurementAttribute>();

        // Common measurements for all specializations
        var commonMeasurements = _measurementAttributes.Where(m => 
            m.NameEn.Contains("Blood Pressure") || 
            m.NameEn.Contains("Heart Rate") || 
            m.NameEn.Contains("Body Temperature") ||
            m.NameEn.Contains("Weight") ||
            m.NameEn.Contains("Height")).ToList();

        foreach (var specialization in _specializations)
        {
            // Add common measurements
            foreach (var measurement in commonMeasurements)
            {
                specializationMeasurements.Add(new SpecializationMeasurementAttribute
                {
                    SpecializationId = specialization.Id,
                    MeasurementAttributeId = measurement.Id,
                    DefaultIsRequired = true,
                    DefaultDisplayOrder = commonMeasurements.IndexOf(measurement) + 1
                });
            }

            // Add specialization-specific measurements
            var specificMeasurements = specialization.NameEn switch
            {
                "Cardiology" => _measurementAttributes.Where(m => m.NameEn.Contains("Blood Pressure")).ToList(),
                "Internal Medicine" => _measurementAttributes.Where(m => m.NameEn.Contains("Blood Sugar")).ToList(),
                "Pediatrics" => _measurementAttributes.Where(m => m.NameEn.Contains("Weight") || m.NameEn.Contains("Height")).ToList(),
                _ => new List<MeasurementAttribute>()
            };

            var displayOrder = commonMeasurements.Count + 1;
            foreach (var measurement in specificMeasurements)
            {
                if (!specializationMeasurements.Any(sm => sm.SpecializationId == specialization.Id && sm.MeasurementAttributeId == measurement.Id))
                {
                    specializationMeasurements.Add(new SpecializationMeasurementAttribute
                    {
                        SpecializationId = specialization.Id,
                        MeasurementAttributeId = measurement.Id,
                        DefaultIsRequired = false,
                        DefaultDisplayOrder = displayOrder++
                    });
                }
            }
        }

        await _context.SpecializationMeasurementAttributes.AddRangeAsync(specializationMeasurements);
        _logger.LogInformation("Created {Count} specialization measurement attributes", specializationMeasurements.Count);
    }
}
