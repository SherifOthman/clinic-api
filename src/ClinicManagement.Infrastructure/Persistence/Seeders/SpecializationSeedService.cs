using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class SpecializationSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SpecializationSeedService> _logger;

    public SpecializationSeedService(
        IApplicationDbContext context,
        ILogger<SpecializationSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedSpecializationsAsync()
    {
        var hasSpecializations = await _context.Specializations.AnyAsync();
        if (hasSpecializations)
        {
            _logger.LogInformation("Specializations already seeded");
            return;
        }

        var specializations = new[]
        {
            new Specialization { NameEn = "General Practice", NameAr = "طب عام", DescriptionEn = "General medical practice", DescriptionAr = "ممارسة طبية عامة" },
            new Specialization { NameEn = "Pediatrics", NameAr = "طب الأطفال", DescriptionEn = "Medical care for children", DescriptionAr = "الرعاية الطبية للأطفال" },
            new Specialization { NameEn = "Cardiology", NameAr = "أمراض القلب", DescriptionEn = "Heart and cardiovascular system", DescriptionAr = "القلب والجهاز القلبي الوعائي" },
            new Specialization { NameEn = "Dermatology", NameAr = "الأمراض الجلدية", DescriptionEn = "Skin conditions and diseases", DescriptionAr = "الأمراض والحالات الجلدية" },
            new Specialization { NameEn = "Orthopedics", NameAr = "جراحة العظام", DescriptionEn = "Bones, joints, and muscles", DescriptionAr = "العظام والمفاصل والعضلات" },
            new Specialization { NameEn = "Gynecology", NameAr = "أمراض النساء", DescriptionEn = "Women's reproductive health", DescriptionAr = "صحة المرأة الإنجابية" },
            new Specialization { NameEn = "Ophthalmology", NameAr = "طب العيون", DescriptionEn = "Eye care and vision", DescriptionAr = "العناية بالعيون والبصر" },
            new Specialization { NameEn = "Dentistry", NameAr = "طب الأسنان", DescriptionEn = "Oral health and dental care", DescriptionAr = "صحة الفم والعناية بالأسنان" },
            new Specialization { NameEn = "Psychiatry", NameAr = "الطب النفسي", DescriptionEn = "Mental health", DescriptionAr = "الصحة النفسية" },
            new Specialization { NameEn = "Neurology", NameAr = "طب الأعصاب", DescriptionEn = "Nervous system disorders", DescriptionAr = "اضطرابات الجهاز العصبي" },
            new Specialization { NameEn = "ENT", NameAr = "أنف وأذن وحنجرة", DescriptionEn = "Ear, Nose, and Throat", DescriptionAr = "الأذن والأنف والحنجرة" },
            new Specialization { NameEn = "Urology", NameAr = "المسالك البولية", DescriptionEn = "Urinary tract and male reproductive system", DescriptionAr = "الجهاز البولي والجهاز التناسلي الذكري" },
            new Specialization { NameEn = "Endocrinology", NameAr = "الغدد الصماء", DescriptionEn = "Hormones and metabolism", DescriptionAr = "الهرمونات والتمثيل الغذائي" },
            new Specialization { NameEn = "Gastroenterology", NameAr = "الجهاز الهضمي", DescriptionEn = "Digestive system", DescriptionAr = "الجهاز الهضمي" },
            new Specialization { NameEn = "Pulmonology", NameAr = "أمراض الرئة", DescriptionEn = "Respiratory system", DescriptionAr = "الجهاز التنفسي" }
        };

        _context.Specializations.AddRange(specializations);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} specializations", specializations.Length);
    }
}
