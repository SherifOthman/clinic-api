using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Infrastructure.Services;

public interface IComprehensiveSeedService
{
    Task SeedAllDataAsync();
}

public class ComprehensiveSeedService : IComprehensiveSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ICodeGeneratorService _codeGenerator;
    private readonly ILogger<ComprehensiveSeedService> _logger;

    // Store created entities for relationships
    private List<Specialization> _specializations = new();
    private List<MeasurementAttribute> _measurementAttributes = new();
    private List<ChronicDisease> _chronicDiseases = new();
    private List<AppointmentType> _appointmentTypes = new();
    private List<SubscriptionPlan> _subscriptionPlans = new();
    private List<User> _users = new();
    private List<Clinic> _clinics = new();
    private List<ClinicBranch> _clinicBranches = new();
    private List<Staff> _staff = new();
    private List<Doctor> _doctors = new();
    private List<Patient> _patients = new();
    private List<ClinicPatient> _clinicPatients = new();
    private List<Medicine> _medicines = new();
    private List<MedicalSupply> _medicalSupplies = new();
    private List<MedicalService> _medicalServices = new();
    private List<Appointment> _appointments = new();
    private List<LabTest> _labTests = new();
    private List<RadiologyTest> _radiologyTests = new();
    private List<MedicalVisit> _medicalVisits = new();

    public ComprehensiveSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ICodeGeneratorService codeGenerator,
        ILogger<ComprehensiveSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task SeedAllDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting comprehensive data seeding...");

            // Check if data already exists
            if (await _context.Specializations.AnyAsync())
            {
                _logger.LogInformation("Seed data already exists. Skipping seeding.");
                return;
            }

            // Seed in order of dependencies
            await SeedRolesAsync();
            await SeedSpecializationsAsync();
            await SeedMeasurementAttributesAsync();
            await SeedChronicDiseasesAsync();
            await SeedAppointmentTypesAsync();
            await SeedSubscriptionPlansAsync();
            await SeedUsersAsync();
            await SeedClinicsAndBranchesAsync();
            await SeedStaffAsync();
            await SeedDoctorsAsync();
            await SeedClinicBranchAppointmentPricesAsync();
            await SeedSpecializationMeasurementAttributesAsync();
            await SeedDoctorMeasurementAttributesAsync();
            await SeedPatientsAsync();
            await SeedClinicPatientsAsync();
            await SeedClinicPatientChronicDiseasesAsync();
            await SeedMedicinesAsync();
            await SeedMedicalSuppliesAsync();
            await SeedMedicalServicesAsync();
            await SeedAppointmentsAsync();
            await SeedInvoicesAndPaymentsAsync();
            await SeedClinicBranchWorkingDaysAsync();
            await SeedLabTestsAsync();
            await SeedRadiologyTestsAsync();
            await SeedMedicalVisitsAsync();
            await SeedPrescriptionsAsync();
            await SeedClinicPatientPhonesAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Comprehensive data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during comprehensive data seeding");
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
            "ClinicAdmin",
            "Doctor",
            "Nurse",
            "Receptionist",
            "Patient"
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
            new SubscriptionPlan
            {
                Name = "Basic Plan",
                Description = "Essential features for small clinics",
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
                IsPopular = true,
                DisplayOrder = 1
            },
            new SubscriptionPlan
            {
                Name = "Professional Plan",
                Description = "Advanced features for established clinics",
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
                DisplayOrder = 2
            }
        };

        await _context.SubscriptionPlans.AddRangeAsync(subscriptionPlans);
        _subscriptionPlans.AddRange(subscriptionPlans);
        _logger.LogInformation("Added {Count} subscription plans", subscriptionPlans.Length);
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        // Create SuperAdmin user
        var superAdmin = new User
        {
            UserName = "superadmin@clinic.com",
            Email = "superadmin@clinic.com",
            FullName = "System Administrator",
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            _users.Add(superAdmin);
        }

        // Create Clinic Owner users
        var clinicOwners = new[]
        {
            new { User = new User { UserName = "owner1@alshifa.com", Email = "owner1@alshifa.com", FullName = "Dr. Ahmed Al-Rashid", EmailConfirmed = true }, Password = "Owner123!" }
        };

        foreach (var owner in clinicOwners)
        {
            var createResult = await _userManager.CreateAsync(owner.User, owner.Password);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(owner.User, "ClinicOwner");
                _users.Add(owner.User);
            }
        }

        // Create Doctor users
        var doctors = new[]
        {
            new { User = new User { UserName = "dr.ahmed@alshifa.com", Email = "dr.ahmed@alshifa.com", FullName = "Dr. Ahmed Hassan", EmailConfirmed = true }, Password = "Doctor123!" },
            new { User = new User { UserName = "dr.fatima@alshifa.com", Email = "dr.fatima@alshifa.com", FullName = "Dr. Fatima Al-Zahra", EmailConfirmed = true }, Password = "Doctor123!" },
            new { User = new User { UserName = "dr.omar@alshifa.com", Email = "dr.omar@alshifa.com", FullName = "Dr. Omar Mahmoud", EmailConfirmed = true }, Password = "Doctor123!" },
            new { User = new User { UserName = "dr.sara@alshifa.com", Email = "dr.sara@alshifa.com", FullName = "Dr. Sara Ibrahim", EmailConfirmed = true }, Password = "Doctor123!" }
        };

        foreach (var doctor in doctors)
        {
            var createResult = await _userManager.CreateAsync(doctor.User, doctor.Password);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(doctor.User, "Doctor");
                _users.Add(doctor.User);
            }
        }

        // Create Receptionist users
        var receptionists = new[]
        {
            new { User = new User { UserName = "reception1@alshifa.com", Email = "reception1@alshifa.com", FullName = "Aisha Al-Zahra", EmailConfirmed = true }, Password = "Reception123!" },
            new { User = new User { UserName = "reception2@alshifa.com", Email = "reception2@alshifa.com", FullName = "Khadija Al-Hassan", EmailConfirmed = true }, Password = "Reception123!" }
        };

        foreach (var receptionist in receptionists)
        {
            var createResult = await _userManager.CreateAsync(receptionist.User, receptionist.Password);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(receptionist.User, "Receptionist");
                _users.Add(receptionist.User);
            }
        }

        _logger.LogInformation("Created {Count} users", _users.Count);
    }

    private async Task SeedClinicsAndBranchesAsync()
    {
        _logger.LogInformation("Seeding clinics and branches...");

        var clinicOwner = _users.FirstOrDefault(u => u.Email == "owner1@alshifa.com");
        var basicPlan = _subscriptionPlans.FirstOrDefault(p => p.Name == "Basic Plan");

        if (clinicOwner != null && basicPlan != null)
        {
            var clinic = new Clinic
            {
                Name = "Al-Shifa Medical Center",
                OwnerUserId = clinicOwner.Id,
                SubscriptionPlanId = basicPlan.Id
            };

            await _context.Clinics.AddAsync(clinic);
            _clinics.Add(clinic);

            var branches = new[]
            {
                new ClinicBranch
                {
                    ClinicId = clinic.Id,
                    Name = "Main Branch",
                    Address = "King Fahd Road, Riyadh, Saudi Arabia"
                },
                new ClinicBranch
                {
                    ClinicId = clinic.Id,
                    Name = "North Branch",
                    Address = "Olaya District, Riyadh, Saudi Arabia"
                }
            };

            await _context.ClinicBranches.AddRangeAsync(branches);
            _clinicBranches.AddRange(branches);
        }

        _logger.LogInformation("Created {ClinicCount} clinics with {BranchCount} branches", _clinics.Count, _clinicBranches.Count);
    }

    private async Task SeedStaffAsync()
    {
        _logger.LogInformation("Seeding staff...");

        if (!_clinics.Any() || !_users.Any()) return;

        var clinic = _clinics.First();
        var staffMembers = new List<Staff>();

        // Link clinic owner
        var clinicOwner = _users.FirstOrDefault(u => u.Email == "owner1@alshifa.com");
        if (clinicOwner != null)
        {
            staffMembers.Add(new Staff
            {
                UserId = clinicOwner.Id,
                ClinicId = clinic.Id,
                Role = StaffRole.ClinicOwner,
                IsActive = true
            });
        }

        // Link doctors
        var doctorUsers = _users.Where(u => u.Email.Contains("dr.")).ToList();
        foreach (var doctorUser in doctorUsers)
        {
            staffMembers.Add(new Staff
            {
                UserId = doctorUser.Id,
                ClinicId = clinic.Id,
                Role = StaffRole.Doctor,
                IsActive = true
            });
        }

        // Link receptionists
        var receptionistUsers = _users.Where(u => u.Email.Contains("reception")).ToList();
        foreach (var receptionistUser in receptionistUsers)
        {
            staffMembers.Add(new Staff
            {
                UserId = receptionistUser.Id,
                ClinicId = clinic.Id,
                Role = StaffRole.Receptionist,
                IsActive = true
            });
        }

        await _context.Staff.AddRangeAsync(staffMembers);
        _staff.AddRange(staffMembers);
        _logger.LogInformation("Created {Count} staff members", staffMembers.Count);
    }

    private async Task SeedDoctorsAsync()
    {
        _logger.LogInformation("Seeding doctors...");

        if (!_staff.Any() || !_specializations.Any()) return;

        var doctorStaff = _staff.Where(s => s.Role == StaffRole.Doctor).ToList();
        var doctors = new List<Doctor>();

        var specializationIndex = 0;
        foreach (var staff in doctorStaff)
        {
            var specialization = _specializations[specializationIndex % _specializations.Count];
            
            var doctor = new Doctor
            {
                SpecializationId = specialization.Id,
                YearsOfExperience = (short)Random.Shared.Next(2, 20)
            };

            doctors.Add(doctor);

            // Link the doctor profile to the staff member
            staff.DoctorProfileId = doctor.Id;

            specializationIndex++;
        }

        await _context.Doctors.AddRangeAsync(doctors);
        _doctors.AddRange(doctors);
        _logger.LogInformation("Created {Count} doctors", doctors.Count);
    }

    private async Task SeedClinicBranchAppointmentPricesAsync()
    {
        _logger.LogInformation("Seeding clinic branch appointment prices...");

        if (!_clinicBranches.Any() || !_appointmentTypes.Any()) return;

        var appointmentPrices = new List<ClinicBranchAppointmentPrice>();

        foreach (var branch in _clinicBranches)
        {
            foreach (var appointmentType in _appointmentTypes)
            {
                decimal price = appointmentType.NameEn switch
                {
                    "Initial Consultation" => 200.00m,
                    "Follow-up Visit" => 150.00m,
                    "Emergency Visit" => 300.00m,
                    "Annual Checkup" => 220.00m,
                    "Vaccination" => 100.00m,
                    "Physical Therapy" => 180.00m,
                    "Chronic Disease Management" => 200.00m,
                    "Preventive Care" => 180.00m,
                    _ => 150.00m
                };

                appointmentPrices.Add(new ClinicBranchAppointmentPrice
                {
                    ClinicBranchId = branch.Id,
                    AppointmentTypeId = appointmentType.Id,
                    Price = price
                });
            }
        }

        await _context.ClinicBranchAppointmentPrices.AddRangeAsync(appointmentPrices);
        _logger.LogInformation("Created {Count} appointment prices", appointmentPrices.Count);
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

    private async Task SeedDoctorMeasurementAttributesAsync()
    {
        _logger.LogInformation("Seeding doctor measurement attributes...");

        if (!_doctors.Any()) return;

        var doctorMeasurements = new List<DoctorMeasurementAttribute>();

        foreach (var doctor in _doctors)
        {
            // Get specialization default measurements
            var specializationMeasurements = await _context.SpecializationMeasurementAttributes
                .Where(sm => sm.SpecializationId == doctor.SpecializationId)
                .ToListAsync();

            foreach (var sm in specializationMeasurements)
            {
                doctorMeasurements.Add(new DoctorMeasurementAttribute
                {
                    DoctorId = doctor.Id,
                    MeasurementAttributeId = sm.MeasurementAttributeId,
                    IsRequired = sm.DefaultIsRequired,
                    DisplayOrder = sm.DefaultDisplayOrder
                });
            }
        }

        await _context.DoctorMeasurementAttributes.AddRangeAsync(doctorMeasurements);
        _logger.LogInformation("Created {Count} doctor measurement attributes", doctorMeasurements.Count);
    }

    private async Task SeedPatientsAsync()
    {
        _logger.LogInformation("Seeding patients...");

        var patients = new[]
        {
            new Patient { FullName = "Mohammed Al-Saud", DateOfBirth = new DateTime(1985, 3, 15), Gender = Gender.Male, City = "Riyadh", Address = "Al-Malaz District" },
            new Patient { FullName = "Aisha Al-Zahra", DateOfBirth = new DateTime(1990, 7, 22), Gender = Gender.Female, City = "Riyadh", Address = "Al-Nakheel District" },
            new Patient { FullName = "Abdullah Al-Rashid", DateOfBirth = new DateTime(1978, 12, 8), Gender = Gender.Male, City = "Riyadh", Address = "Al-Olaya District" },
            new Patient { FullName = "Khadija Al-Kindi", DateOfBirth = new DateTime(1995, 5, 18), Gender = Gender.Female, City = "Riyadh", Address = "Al-Sahafa District" },
            new Patient { FullName = "Yusuf Al-Mahmoud", DateOfBirth = new DateTime(2010, 9, 12), Gender = Gender.Male, City = "Riyadh", Address = "Al-Wurud District" },
            new Patient { FullName = "Zainab Al-Hassan", DateOfBirth = new DateTime(1982, 11, 25), Gender = Gender.Female, City = "Riyadh", Address = "Al-Rabwa District" },
            new Patient { FullName = "Hassan Al-Ibrahim", DateOfBirth = new DateTime(1975, 4, 30), Gender = Gender.Male, City = "Jeddah", Address = "Al-Hamra District" },
            new Patient { FullName = "Fatima Al-Qasimi", DateOfBirth = new DateTime(1988, 8, 14), Gender = Gender.Female, City = "Jeddah", Address = "Al-Balad District" }
        };

        await _context.Patients.AddRangeAsync(patients);
        _patients.AddRange(patients);
        _logger.LogInformation("Created {Count} patients", patients.Length);
    }

    private async Task SeedClinicPatientsAsync()
    {
        _logger.LogInformation("Seeding clinic patients...");

        var clinic = _clinics.FirstOrDefault();
        if (clinic == null) return;

        var clinicPatients = new List<ClinicPatient>();
        var patientCounter = 1;

        foreach (var patient in _patients)
        {
            var clinicPatient = new ClinicPatient
            {
                PatientNumber = $"PAT{DateTime.UtcNow.Year}{patientCounter:D4}",
                ClinicId = clinic.Id,
                PatientId = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender,
                City = patient.City,
                Address = patient.Address,
                DateOfBirth = patient.DateOfBirth,
                MedicalFileNumber = $"MF{patientCounter:D6}"
            };

            clinicPatients.Add(clinicPatient);
            patientCounter++;
        }

        await _context.ClinicPatients.AddRangeAsync(clinicPatients);
        _clinicPatients.AddRange(clinicPatients);
        _logger.LogInformation("Created {Count} clinic patient records", clinicPatients.Count);
    }

    private async Task SeedClinicPatientChronicDiseasesAsync()
    {
        _logger.LogInformation("Seeding clinic patient chronic diseases...");

        if (!_clinicPatients.Any() || !_chronicDiseases.Any()) return;

        var patientDiseases = new List<ClinicPatientChronicDisease>();

        // Assign chronic diseases to some patients
        for (int i = 0; i < _clinicPatients.Count; i++)
        {
            var patient = _clinicPatients[i];
            
            // 30% of patients have chronic diseases
            if (i % 3 == 0)
            {
                var disease = _chronicDiseases[i % _chronicDiseases.Count];
                
                patientDiseases.Add(new ClinicPatientChronicDisease
                {
                    ClinicPatientId = patient.Id,
                    ChronicDiseaseId = disease.Id,
                    DiagnosedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 1095)), // 1 month to 3 years ago
                    Status = "Active",
                    Notes = $"Patient diagnosed with {disease.NameEn}. Regular monitoring required."
                });
            }
        }

        await _context.ClinicPatientChronicDiseases.AddRangeAsync(patientDiseases);
        _logger.LogInformation("Created {Count} patient chronic disease records", patientDiseases.Count);
    }

    private async Task SeedMedicinesAsync()
    {
        _logger.LogInformation("Seeding medicines...");

        if (!_clinicBranches.Any()) return;

        var medicines = new List<Medicine>();
        var medicineData = new[]
        {
            new { Name = "Paracetamol 500mg", BoxPrice = 15.50m, StripsPerBox = 10, Stock = 500, MinStock = 50 },
            new { Name = "Ibuprofen 400mg", BoxPrice = 22.00m, StripsPerBox = 8, Stock = 300, MinStock = 30 },
            new { Name = "Amoxicillin 500mg", BoxPrice = 35.75m, StripsPerBox = 12, Stock = 200, MinStock = 25 },
            new { Name = "Metformin 850mg", BoxPrice = 28.90m, StripsPerBox = 10, Stock = 150, MinStock = 20 },
            new { Name = "Amlodipine 5mg", BoxPrice = 42.50m, StripsPerBox = 14, Stock = 180, MinStock = 25 }
        };

        foreach (var branch in _clinicBranches)
        {
            foreach (var med in medicineData)
            {
                medicines.Add(new Medicine
                {
                    ClinicBranchId = branch.Id,
                    Name = med.Name,
                    Description = $"High quality {med.Name} for medical treatment",
                    Manufacturer = "PharmaCorp Ltd.",
                    BoxPrice = med.BoxPrice,
                    StripsPerBox = med.StripsPerBox,
                    TotalStripsInStock = med.Stock,
                    MinimumStockLevel = med.MinStock,
                    ReorderLevel = med.MinStock * 2,
                    ExpiryDate = DateTime.UtcNow.AddMonths(Random.Shared.Next(6, 24)),
                    BatchNumber = $"B{DateTime.UtcNow.Year}{Random.Shared.Next(1000, 9999)}"
                });
            }
        }

        await _context.Medicines.AddRangeAsync(medicines);
        _medicines.AddRange(medicines);
        _logger.LogInformation("Created {Count} medicines across all branches", medicines.Count);
    }

    private async Task SeedMedicalSuppliesAsync()
    {
        _logger.LogInformation("Seeding medical supplies...");

        if (!_clinicBranches.Any()) return;

        var supplies = new List<MedicalSupply>();
        var supplyData = new[]
        {
            new { Name = "Disposable Syringes 5ml", Price = 0.75m, Stock = 1000, MinStock = 100 },
            new { Name = "Surgical Gloves (Box)", Price = 25.00m, Stock = 50, MinStock = 10 },
            new { Name = "Face Masks (Box)", Price = 15.50m, Stock = 80, MinStock = 15 },
            new { Name = "Bandages 10cm", Price = 8.25m, Stock = 200, MinStock = 25 },
            new { Name = "Alcohol Swabs (Pack)", Price = 12.00m, Stock = 150, MinStock = 20 }
        };

        foreach (var branch in _clinicBranches)
        {
            foreach (var supply in supplyData)
            {
                supplies.Add(new MedicalSupply
                {
                    ClinicBranchId = branch.Id,
                    Name = supply.Name,
                    UnitPrice = supply.Price,
                    QuantityInStock = supply.Stock,
                    MinimumStockLevel = supply.MinStock
                });
            }
        }

        await _context.MedicalSupplies.AddRangeAsync(supplies);
        _medicalSupplies.AddRange(supplies);
        _logger.LogInformation("Created {Count} medical supplies across all branches", supplies.Count);
    }

    private async Task SeedMedicalServicesAsync()
    {
        _logger.LogInformation("Seeding medical services...");

        if (!_clinicBranches.Any()) return;

        var services = new List<MedicalService>();
        var serviceData = new[]
        {
            new { Name = "General Consultation", Price = 150.00m, IsOperation = false },
            new { Name = "Specialist Consultation", Price = 250.00m, IsOperation = false },
            new { Name = "Emergency Consultation", Price = 300.00m, IsOperation = false },
            new { Name = "Follow-up Visit", Price = 100.00m, IsOperation = false },
            new { Name = "Blood Test (Complete)", Price = 80.00m, IsOperation = false },
            new { Name = "X-Ray (Chest)", Price = 120.00m, IsOperation = false },
            new { Name = "ECG", Price = 100.00m, IsOperation = false },
            new { Name = "Vaccination (Adult)", Price = 75.00m, IsOperation = false }
        };

        foreach (var branch in _clinicBranches)
        {
            foreach (var service in serviceData)
            {
                services.Add(new MedicalService
                {
                    ClinicBranchId = branch.Id,
                    Name = service.Name,
                    DefaultPrice = service.Price,
                    IsOperation = service.IsOperation
                });
            }
        }

        await _context.MedicalServices.AddRangeAsync(services);
        _medicalServices.AddRange(services);
        _logger.LogInformation("Created {Count} medical services across all branches", services.Count);
    }

    private async Task SeedAppointmentsAsync()
    {
        _logger.LogInformation("Seeding appointments...");

        if (!_clinicPatients.Any() || !_staff.Any() || !_appointmentTypes.Any()) return;

        var appointments = new List<Appointment>();
        var appointmentCounter = 1;
        var doctorStaff = _staff.Where(s => s.Role == StaffRole.Doctor).ToList();

        if (!doctorStaff.Any()) return;

        // Create appointments for the past 30 days and next 30 days
        for (int dayOffset = -30; dayOffset <= 30; dayOffset++)
        {
            var appointmentDate = DateTime.UtcNow.Date.AddDays(dayOffset);
            
            // Skip weekends for most appointments
            if (appointmentDate.DayOfWeek == DayOfWeek.Sunday) continue;

            // 2-5 appointments per day
            var appointmentsPerDay = Random.Shared.Next(2, 6);
            
            for (int i = 0; i < appointmentsPerDay; i++)
            {
                var patient = _clinicPatients[Random.Shared.Next(_clinicPatients.Count)];
                var doctor = doctorStaff[Random.Shared.Next(doctorStaff.Count)];
                var appointmentType = _appointmentTypes[Random.Shared.Next(_appointmentTypes.Count)];
                var branch = _clinicBranches[Random.Shared.Next(_clinicBranches.Count)];
                
                var appointmentTime = appointmentDate.AddHours(9 + i * 2); // 9 AM, 11 AM, 1 PM, 3 PM, 5 PM
                
                var status = dayOffset < 0 ? 
                    (Random.Shared.Next(10) < 8 ? AppointmentStatus.Completed : AppointmentStatus.Cancelled) :
                    AppointmentStatus.Confirmed;

                var price = Random.Shared.Next(150, 401); // 150-400 SAR
                var discount = Random.Shared.Next(10) < 3 ? Random.Shared.Next(10, 51) : 0; // 30% chance of 10-50 SAR discount
                var paidAmount = status == AppointmentStatus.Completed ? price - discount : 
                                status == AppointmentStatus.Confirmed ? Random.Shared.Next(0, price - discount + 1) : 0;

                appointments.Add(new Appointment
                {
                    AppointmentNumber = $"APT{DateTime.UtcNow.Year}{appointmentCounter:D6}",
                    ClinicBranchId = branch.Id,
                    ClinicPatientId = patient.Id,
                    DoctorId = doctor.Id,
                    AppointmentTypeId = appointmentType.Id,
                    AppointmentDate = appointmentTime,
                    QueueNumber = (short)(i + 1),
                    Status = status,
                    FinalPrice = price,
                    DiscountAmount = discount,
                    PaidAmount = paidAmount
                });

                appointmentCounter++;
            }
        }

        await _context.Appointments.AddRangeAsync(appointments);
        _appointments.AddRange(appointments);
        _logger.LogInformation("Created {Count} appointments", appointments.Count);
    }

    private async Task SeedInvoicesAndPaymentsAsync()
    {
        _logger.LogInformation("Seeding invoices and payments...");

        if (!_appointments.Any()) return;

        var invoices = new List<Invoice>();
        var invoiceItems = new List<InvoiceItem>();
        var payments = new List<Payment>();
        var invoiceCounter = 1;

        // Create invoices for completed appointments
        var completedAppointments = _appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();

        foreach (var appointment in completedAppointments)
        {
            var clinic = _clinics.FirstOrDefault();
            if (clinic == null) continue;

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV{DateTime.UtcNow.Year}{invoiceCounter:D6}",
                ClinicId = clinic.Id,
                ClinicPatientId = appointment.ClinicPatientId,
                TotalAmount = appointment.FinalPrice,
                Discount = appointment.DiscountAmount,
                TaxAmount = appointment.FinalPrice * 0.15m, // 15% VAT
                Status = InvoiceStatus.FullyPaid,
                IssuedDate = appointment.AppointmentDate.Date,
                DueDate = appointment.AppointmentDate.Date.AddDays(30),
                Notes = "Invoice for medical services"
            };

            invoices.Add(invoice);

            // Add consultation fee as invoice item
            if (_medicalServices.Any())
            {
                var consultationService = _medicalServices.FirstOrDefault(s => s.Name.Contains("Consultation"));
                if (consultationService != null)
                {
                    invoiceItems.Add(new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        MedicalServiceId = consultationService.Id,
                        Quantity = 1,
                        UnitPrice = appointment.FinalPrice
                    });
                }
            }

            // Add some medicines (40% chance)
            if (Random.Shared.Next(10) < 4 && _medicines.Any())
            {
                var medicine = _medicines[Random.Shared.Next(_medicines.Count)];
                var quantity = Random.Shared.Next(1, 4);

                invoiceItems.Add(new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    MedicineId = medicine.Id,
                    Quantity = quantity,
                    UnitPrice = medicine.StripPrice,
                    SaleUnit = SaleUnit.Strip
                });
            }

            // Create payment
            payments.Add(new Payment
            {
                InvoiceId = invoice.Id,
                PaymentDate = appointment.AppointmentDate.Date,
                Amount = invoice.TotalAmount + invoice.TaxAmount - invoice.Discount,
                PaymentMethod = GetRandomPaymentMethod(),
                ReferenceNumber = $"PAY{DateTime.UtcNow.Year}{invoiceCounter:D6}",
                Status = PaymentStatus.Paid
            });

            invoiceCounter++;
        }

        await _context.Invoices.AddRangeAsync(invoices);
        await _context.InvoiceItems.AddRangeAsync(invoiceItems);
        await _context.Payments.AddRangeAsync(payments);
        _logger.LogInformation("Created {Count} invoices with {ItemCount} items and {PaymentCount} payments", 
            invoices.Count, invoiceItems.Count, payments.Count);
    }

    // Helper methods
    private PaymentMethod GetRandomPaymentMethod()
    {
        var methods = new[] { PaymentMethod.Cash, PaymentMethod.CreditCard, PaymentMethod.DebitCard, PaymentMethod.BankTransfer };
        return methods[Random.Shared.Next(methods.Length)];
    }

    private async Task SeedClinicBranchWorkingDaysAsync()
    {
        _logger.LogInformation("Seeding clinic branch working days...");

        if (!_clinicBranches.Any()) return;

        var workingDays = new List<ClinicBranchWorkingDay>();

        foreach (var branch in _clinicBranches)
        {
            // Monday to Friday
            for (int dayOfWeek = 1; dayOfWeek <= 5; dayOfWeek++)
            {
                workingDays.Add(new ClinicBranchWorkingDay
                {
                    ClinicBranchId = branch.Id,
                    DayOfWeek = (DayOfWeek)dayOfWeek,
                    StartTime = new TimeOnly(8, 0), // 8:00 AM
                    EndTime = new TimeOnly(17, 0),  // 5:00 PM
                    IsActive = true
                });
            }

            // Saturday (half day)
            workingDays.Add(new ClinicBranchWorkingDay
            {
                ClinicBranchId = branch.Id,
                DayOfWeek = DayOfWeek.Saturday,
                StartTime = new TimeOnly(8, 0),  // 8:00 AM
                EndTime = new TimeOnly(12, 0),   // 12:00 PM
                IsActive = true
            });
        }

        await _context.ClinicBranchWorkingDays.AddRangeAsync(workingDays);
        _logger.LogInformation("Created {Count} working day schedules", workingDays.Count);
    }

    private async Task SeedLabTestsAsync()
    {
        _logger.LogInformation("Seeding lab tests...");

        var labTests = new[]
        {
            new LabTest { Name = "Complete Blood Count (CBC)", Description = "Full blood analysis including RBC, WBC, platelets", Price = 80.00m },
            new LabTest { Name = "Blood Sugar (Fasting)", Description = "Fasting blood glucose test", Price = 25.00m },
            new LabTest { Name = "HbA1c", Description = "Glycated hemoglobin test for diabetes monitoring", Price = 120.00m },
            new LabTest { Name = "Lipid Profile", Description = "Cholesterol and triglycerides analysis", Price = 90.00m },
            new LabTest { Name = "Liver Function Test", Description = "ALT, AST, bilirubin levels", Price = 100.00m },
            new LabTest { Name = "Kidney Function Test", Description = "Creatinine, urea, electrolytes", Price = 85.00m },
            new LabTest { Name = "Thyroid Function Test", Description = "TSH, T3, T4 levels", Price = 150.00m },
            new LabTest { Name = "Urine Analysis", Description = "Complete urine examination", Price = 40.00m }
        };

        await _context.LabTests.AddRangeAsync(labTests);
        _labTests.AddRange(labTests);
        _logger.LogInformation("Added {Count} lab tests", labTests.Length);
    }

    private async Task SeedRadiologyTestsAsync()
    {
        _logger.LogInformation("Seeding radiology tests...");

        var radiologyTests = new[]
        {
            new RadiologyTest { Name = "Chest X-Ray", Description = "Chest radiograph examination", Price = 120.00m },
            new RadiologyTest { Name = "Abdominal X-Ray", Description = "Abdominal radiograph", Price = 100.00m },
            new RadiologyTest { Name = "CT Scan - Head", Description = "Computed tomography of head", Price = 800.00m },
            new RadiologyTest { Name = "CT Scan - Chest", Description = "Computed tomography of chest", Price = 900.00m },
            new RadiologyTest { Name = "MRI - Brain", Description = "Magnetic resonance imaging of brain", Price = 1500.00m },
            new RadiologyTest { Name = "Ultrasound - Abdomen", Description = "Abdominal ultrasound examination", Price = 200.00m },
            new RadiologyTest { Name = "Echocardiogram", Description = "Heart ultrasound examination", Price = 300.00m },
            new RadiologyTest { Name = "Mammography", Description = "Breast X-ray examination", Price = 250.00m }
        };

        await _context.RadiologyTests.AddRangeAsync(radiologyTests);
        _radiologyTests.AddRange(radiologyTests);
        _logger.LogInformation("Added {Count} radiology tests", radiologyTests.Length);
    }

    private async Task SeedMedicalVisitsAsync()
    {
        _logger.LogInformation("Seeding medical visits...");

        if (!_appointments.Any() || !_doctors.Any()) return;

        var medicalVisits = new List<MedicalVisit>();
        var medicalVisitMeasurements = new List<MedicalVisitMeasurement>();
        var medicalVisitLabTests = new List<MedicalVisitLabTest>();
        var medicalVisitRadiologies = new List<MedicalVisitRadiology>();

        // Create medical visits for completed appointments
        var completedAppointments = _appointments.Where(a => a.Status == AppointmentStatus.Completed).Take(20).ToList();

        foreach (var appointment in completedAppointments)
        {
            var doctor = _doctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
            if (doctor == null) continue;

            var medicalVisit = new MedicalVisit
            {
                AppointmentId = appointment.Id,
                ClinicPatientId = appointment.ClinicPatientId,
                DoctorId = appointment.DoctorId,
                VisitDate = appointment.AppointmentDate,
                ChiefComplaint = GetRandomChiefComplaint(),
                PresentIllnessHistory = "Patient presents with symptoms as described in chief complaint.",
                PhysicalExamination = "Physical examination findings documented.",
                Assessment = "Clinical assessment based on examination and history.",
                Plan = "Treatment plan and follow-up recommendations.",
                Notes = "Additional clinical notes and observations."
            };

            medicalVisits.Add(medicalVisit);

            // Add some measurements
            var doctorMeasurements = await _context.DoctorMeasurementAttributes
                .Where(dm => dm.DoctorId == doctor.Id)
                .Take(3)
                .ToListAsync();

            foreach (var dm in doctorMeasurements)
            {
                medicalVisitMeasurements.Add(new MedicalVisitMeasurement
                {
                    MedicalVisitId = medicalVisit.Id,
                    MeasurementAttributeId = dm.MeasurementAttributeId,
                    Value = GetRandomMeasurementValue(dm.MeasurementAttributeId)
                });
            }

            // Add lab tests (30% chance)
            if (Random.Shared.Next(10) < 3 && _labTests.Any())
            {
                var labTest = _labTests[Random.Shared.Next(_labTests.Count)];
                medicalVisitLabTests.Add(new MedicalVisitLabTest
                {
                    MedicalVisitId = medicalVisit.Id,
                    LabTestId = labTest.Id,
                    Status = "Completed",
                    Results = "Test results within normal limits",
                    Notes = "Lab test completed successfully"
                });
            }

            // Add radiology tests (20% chance)
            if (Random.Shared.Next(10) < 2 && _radiologyTests.Any())
            {
                var radiologyTest = _radiologyTests[Random.Shared.Next(_radiologyTests.Count)];
                medicalVisitRadiologies.Add(new MedicalVisitRadiology
                {
                    MedicalVisitId = medicalVisit.Id,
                    RadiologyTestId = radiologyTest.Id,
                    Status = "Completed",
                    Results = "Imaging results show normal findings",
                    Notes = "Radiology examination completed"
                });
            }
        }

        await _context.MedicalVisits.AddRangeAsync(medicalVisits);
        await _context.MedicalVisitMeasurements.AddRangeAsync(medicalVisitMeasurements);
        await _context.MedicalVisitLabTests.AddRangeAsync(medicalVisitLabTests);
        await _context.MedicalVisitRadiologies.AddRangeAsync(medicalVisitRadiologies);
        
        _medicalVisits.AddRange(medicalVisits);
        _logger.LogInformation("Created {Count} medical visits with measurements and tests", medicalVisits.Count);
    }

    private async Task SeedPrescriptionsAsync()
    {
        _logger.LogInformation("Seeding prescriptions...");

        if (!_medicalVisits.Any() || !_medicines.Any()) return;

        var prescriptions = new List<Prescription>();
        var prescriptionItems = new List<PrescriptionItem>();

        foreach (var medicalVisit in _medicalVisits.Take(15))
        {
            var prescription = new Prescription
            {
                MedicalVisitId = medicalVisit.Id,
                ClinicPatientId = medicalVisit.ClinicPatientId,
                DoctorId = medicalVisit.DoctorId,
                PrescriptionDate = medicalVisit.VisitDate,
                Instructions = "Take medications as prescribed. Follow up if symptoms persist.",
                Status = "Active"
            };

            prescriptions.Add(prescription);

            // Add 1-3 medicines to each prescription
            var medicineCount = Random.Shared.Next(1, 4);
            var selectedMedicines = _medicines.OrderBy(x => Random.Shared.Next()).Take(medicineCount);

            foreach (var medicine in selectedMedicines)
            {
                prescriptionItems.Add(new PrescriptionItem
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = medicine.Id,
                    Dosage = GetRandomDosage(),
                    Frequency = GetRandomFrequency(),
                    Duration = GetRandomDuration(),
                    Instructions = "Take with food if stomach upset occurs"
                });
            }
        }

        await _context.Prescriptions.AddRangeAsync(prescriptions);
        await _context.PrescriptionItems.AddRangeAsync(prescriptionItems);
        _logger.LogInformation("Created {Count} prescriptions with {ItemCount} items", prescriptions.Count, prescriptionItems.Count);
    }

    private async Task SeedClinicPatientPhonesAsync()
    {
        _logger.LogInformation("Seeding clinic patient phones...");

        if (!_clinicPatients.Any()) return;

        var patientPhones = new List<ClinicPatientPhone>();

        foreach (var patient in _clinicPatients)
        {
            // Primary phone
            patientPhones.Add(new ClinicPatientPhone
            {
                ClinicPatientId = patient.Id,
                PhoneNumber = $"+966{Random.Shared.Next(50, 60)}{Random.Shared.Next(1000000, 9999999)}",
                PhoneType = "Mobile",
                IsPrimary = true
            });

            // Secondary phone (50% chance)
            if (Random.Shared.Next(2) == 0)
            {
                patientPhones.Add(new ClinicPatientPhone
                {
                    ClinicPatientId = patient.Id,
                    PhoneNumber = $"+966{Random.Shared.Next(11, 18)}{Random.Shared.Next(1000000, 9999999)}",
                    PhoneType = "Home",
                    IsPrimary = false
                });
            }
        }

        await _context.ClinicPatientPhones.AddRangeAsync(patientPhones);
        _logger.LogInformation("Created {Count} patient phone numbers", patientPhones.Count);
    }

    private string GetRandomChiefComplaint()
    {
        var complaints = new[]
        {
            "Chest pain and shortness of breath",
            "Persistent cough and fever",
            "Abdominal pain and nausea",
            "Headache and dizziness",
            "Joint pain and stiffness",
            "Fatigue and weakness",
            "Skin rash and itching",
            "Back pain and muscle spasms"
        };
        return complaints[Random.Shared.Next(complaints.Length)];
    }

    private string GetRandomMeasurementValue(Guid measurementAttributeId)
    {
        // This is a simplified approach - in reality, you'd match by measurement type
        var values = new[]
        {
            "120", "80", "72", "36.5", "70.5", "175", "23.0", "95", "98", "2"
        };
        return values[Random.Shared.Next(values.Length)];
    }

    private string GetRandomDosage()
    {
        var dosages = new[] { "1 tablet", "2 tablets", "5ml", "10ml", "1 capsule", "2 capsules" };
        return dosages[Random.Shared.Next(dosages.Length)];
    }

    private string GetRandomFrequency()
    {
        var frequencies = new[] { "Once daily", "Twice daily", "Three times daily", "Every 6 hours", "Every 8 hours", "As needed" };
        return frequencies[Random.Shared.Next(frequencies.Length)];
    }

    private string GetRandomDuration()
    {
        var durations = new[] { "3 days", "5 days", "7 days", "10 days", "14 days", "1 month" };
        return durations[Random.Shared.Next(durations.Length)];
    }
}