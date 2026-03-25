using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class ChronicDiseaseSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ChronicDiseaseSeedService> _logger;

    public ChronicDiseaseSeedService(
        IApplicationDbContext context,
        ILogger<ChronicDiseaseSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedChronicDiseasesAsync()
    {
        var hasDiseases = await _context.ChronicDiseases.AnyAsync();
        if (hasDiseases)
        {
            _logger.LogInformation("Chronic diseases already seeded");
            return;
        }

        var diseases = new[]
        {
            new ChronicDisease { NameEn = "Diabetes Type 1", NameAr = "السكري النوع الأول", DescriptionEn = "Insulin-dependent diabetes mellitus", DescriptionAr = "داء السكري المعتمد على الأنسولين" },
            new ChronicDisease { NameEn = "Diabetes Type 2", NameAr = "السكري النوع الثاني", DescriptionEn = "Non-insulin-dependent diabetes mellitus", DescriptionAr = "داء السكري غير المعتمد على الأنسولين" },
            new ChronicDisease { NameEn = "Hypertension", NameAr = "ارتفاع ضغط الدم", DescriptionEn = "High blood pressure", DescriptionAr = "ضغط الدم المرتفع" },
            new ChronicDisease { NameEn = "Asthma", NameAr = "الربو", DescriptionEn = "Chronic respiratory condition", DescriptionAr = "حالة تنفسية مزمنة" },
            new ChronicDisease { NameEn = "COPD", NameAr = "مرض الانسداد الرئوي المزمن", DescriptionEn = "Chronic Obstructive Pulmonary Disease", DescriptionAr = "مرض الانسداد الرئوي المزمن" },
            new ChronicDisease { NameEn = "Heart Disease", NameAr = "أمراض القلب", DescriptionEn = "Cardiovascular disease", DescriptionAr = "أمراض القلب والأوعية الدموية" },
            new ChronicDisease { NameEn = "Coronary Artery Disease", NameAr = "مرض الشريان التاجي", DescriptionEn = "Narrowing of coronary arteries", DescriptionAr = "تضيق الشرايين التاجية" },
            new ChronicDisease { NameEn = "Chronic Kidney Disease", NameAr = "مرض الكلى المزمن", DescriptionEn = "Progressive loss of kidney function", DescriptionAr = "فقدان تدريجي لوظائف الكلى" },
            new ChronicDisease { NameEn = "Arthritis", NameAr = "التهاب المفاصل", DescriptionEn = "Joint inflammation and pain", DescriptionAr = "التهاب وألم المفاصل" },
            new ChronicDisease { NameEn = "Rheumatoid Arthritis", NameAr = "التهاب المفاصل الروماتويدي", DescriptionEn = "Autoimmune joint disease", DescriptionAr = "مرض المفاصل المناعي الذاتي" },
            new ChronicDisease { NameEn = "Osteoporosis", NameAr = "هشاشة العظام", DescriptionEn = "Bone density loss", DescriptionAr = "فقدان كثافة العظام" },
            new ChronicDisease { NameEn = "Thyroid Disorder", NameAr = "اضطراب الغدة الدرقية", DescriptionEn = "Thyroid gland dysfunction", DescriptionAr = "خلل في الغدة الدرقية" },
            new ChronicDisease { NameEn = "Hypothyroidism", NameAr = "قصور الغدة الدرقية", DescriptionEn = "Underactive thyroid", DescriptionAr = "خمول الغدة الدرقية" },
            new ChronicDisease { NameEn = "Hyperthyroidism", NameAr = "فرط نشاط الغدة الدرقية", DescriptionEn = "Overactive thyroid", DescriptionAr = "فرط نشاط الغدة الدرقية" },
            new ChronicDisease { NameEn = "Epilepsy", NameAr = "الصرع", DescriptionEn = "Neurological disorder causing seizures", DescriptionAr = "اضطراب عصبي يسبب النوبات" },
            new ChronicDisease { NameEn = "Depression", NameAr = "الاكتئاب", DescriptionEn = "Persistent mood disorder", DescriptionAr = "اضطراب مزاجي مستمر" },
            new ChronicDisease { NameEn = "Anxiety Disorder", NameAr = "اضطراب القلق", DescriptionEn = "Excessive worry and fear", DescriptionAr = "القلق والخوف المفرط" },
            new ChronicDisease { NameEn = "Migraine", NameAr = "الصداع النصفي", DescriptionEn = "Recurrent severe headaches", DescriptionAr = "صداع شديد متكرر" },
            new ChronicDisease { NameEn = "Celiac Disease", NameAr = "مرض السيلياك", DescriptionEn = "Gluten intolerance", DescriptionAr = "عدم تحمل الغلوتين" },
            new ChronicDisease { NameEn = "Crohn's Disease", NameAr = "داء كرون", DescriptionEn = "Inflammatory bowel disease", DescriptionAr = "مرض التهاب الأمعاء" },
            new ChronicDisease { NameEn = "Ulcerative Colitis", NameAr = "التهاب القولون التقرحي", DescriptionEn = "Inflammatory bowel disease", DescriptionAr = "مرض التهاب الأمعاء" },
            new ChronicDisease { NameEn = "Psoriasis", NameAr = "الصدفية", DescriptionEn = "Chronic skin condition", DescriptionAr = "حالة جلدية مزمنة" },
            new ChronicDisease { NameEn = "Eczema", NameAr = "الأكزيما", DescriptionEn = "Chronic skin inflammation", DescriptionAr = "التهاب الجلد المزمن" },
            new ChronicDisease { NameEn = "Lupus", NameAr = "الذئبة الحمراء", DescriptionEn = "Autoimmune disease", DescriptionAr = "مرض المناعة الذاتية" },
            new ChronicDisease { NameEn = "Multiple Sclerosis", NameAr = "التصلب المتعدد", DescriptionEn = "Neurological autoimmune disease", DescriptionAr = "مرض عصبي مناعي ذاتي" },
            new ChronicDisease { NameEn = "Parkinson's Disease", NameAr = "مرض باركنسون", DescriptionEn = "Progressive neurological disorder", DescriptionAr = "اضطراب عصبي تقدمي" },
            new ChronicDisease { NameEn = "Alzheimer's Disease", NameAr = "مرض الزهايمر", DescriptionEn = "Progressive memory loss", DescriptionAr = "فقدان الذاكرة التقدمي" },
            new ChronicDisease { NameEn = "Hepatitis B", NameAr = "التهاب الكبد ب", DescriptionEn = "Chronic liver infection", DescriptionAr = "عدوى الكبد المزمنة" },
            new ChronicDisease { NameEn = "Hepatitis C", NameAr = "التهاب الكبد سي", DescriptionEn = "Chronic liver infection", DescriptionAr = "عدوى الكبد المزمنة" },
            new ChronicDisease { NameEn = "HIV/AIDS", NameAr = "فيروس نقص المناعة البشرية", DescriptionEn = "Immune system disorder", DescriptionAr = "اضطراب الجهاز المناعي" }
        };

        _context.ChronicDiseases.AddRange(diseases);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} chronic diseases", diseases.Length);
    }
}
