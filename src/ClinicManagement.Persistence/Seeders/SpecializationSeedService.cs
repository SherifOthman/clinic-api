using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

public class SpecializationSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SpecializationSeedService> _logger;

    public SpecializationSeedService(ApplicationDbContext context, ILogger<SpecializationSeedService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedSpecializationsAsync()
    {
        if (await _context.Set<Specialization>().AnyAsync()) return;

        _context.Set<Specialization>().AddRange(Items);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} specializations", Items.Length);
    }

    // ── Seed data (replaces specializations.json) ─────────────────────────────

    private static readonly Specialization[] Items =
    [
        new() { NameEn = "General Practice",          NameAr = "الطب العام",                DescriptionEn = "Primary care and general medicine",                    DescriptionAr = "الرعاية الأولية والطب العام" },
        new() { NameEn = "Internal Medicine",         NameAr = "الطب الباطني",              DescriptionEn = "Diagnosis and treatment of adult diseases",             DescriptionAr = "تشخيص وعلاج أمراض البالغين" },
        new() { NameEn = "Cardiology",                NameAr = "أمراض القلب",               DescriptionEn = "Heart and cardiovascular system",                      DescriptionAr = "القلب والجهاز القلبي الوعائي" },
        new() { NameEn = "Dermatology",               NameAr = "الأمراض الجلدية",           DescriptionEn = "Skin, hair, and nail conditions",                      DescriptionAr = "أمراض الجلد والشعر والأظافر" },
        new() { NameEn = "Orthopedics",               NameAr = "جراحة العظام",              DescriptionEn = "Musculoskeletal system and bone surgery",               DescriptionAr = "الجهاز العضلي الهيكلي وجراحة العظام" },
        new() { NameEn = "Pediatrics",                NameAr = "طب الأطفال",                DescriptionEn = "Medical care for infants, children, and adolescents",   DescriptionAr = "الرعاية الطبية للرضع والأطفال والمراهقين" },
        new() { NameEn = "Obstetrics & Gynecology",   NameAr = "أمراض النساء والتوليد",     DescriptionEn = "Women's reproductive health and childbirth",            DescriptionAr = "صحة المرأة الإنجابية والولادة" },
        new() { NameEn = "Ophthalmology",             NameAr = "طب وجراحة العيون",          DescriptionEn = "Eye diseases and vision care",                         DescriptionAr = "أمراض العيون ورعاية البصر" },
        new() { NameEn = "ENT",                       NameAr = "أمراض الأنف والأذن والحنجرة", DescriptionEn = "Ear, nose, and throat conditions",                   DescriptionAr = "أمراض الأذن والأنف والحنجرة" },
        new() { NameEn = "Neurology",                 NameAr = "طب الأعصاب",                DescriptionEn = "Nervous system disorders",                             DescriptionAr = "اضطرابات الجهاز العصبي" },
        new() { NameEn = "Psychiatry",                NameAr = "الطب النفسي",               DescriptionEn = "Mental health and behavioral disorders",               DescriptionAr = "الصحة النفسية والاضطرابات السلوكية" },
        new() { NameEn = "Urology",                   NameAr = "المسالك البولية",            DescriptionEn = "Urinary tract and male reproductive system",           DescriptionAr = "المسالك البولية والجهاز التناسلي الذكري" },
        new() { NameEn = "Gastroenterology",          NameAr = "أمراض الجهاز الهضمي",       DescriptionEn = "Digestive system diseases",                            DescriptionAr = "أمراض الجهاز الهضمي" },
        new() { NameEn = "Endocrinology",             NameAr = "الغدد الصماء",              DescriptionEn = "Hormonal and metabolic disorders",                     DescriptionAr = "الاضطرابات الهرمونية والأيضية" },
        new() { NameEn = "Pulmonology",               NameAr = "أمراض الصدر والجهاز التنفسي", DescriptionEn = "Respiratory system diseases",                       DescriptionAr = "أمراض الجهاز التنفسي" },
        new() { NameEn = "Rheumatology",              NameAr = "أمراض الروماتيزم",           DescriptionEn = "Autoimmune and joint diseases",                        DescriptionAr = "أمراض المناعة الذاتية والمفاصل" },
        new() { NameEn = "Nephrology",                NameAr = "أمراض الكلى",               DescriptionEn = "Kidney diseases and renal care",                       DescriptionAr = "أمراض الكلى والرعاية الكلوية" },
        new() { NameEn = "Oncology",                  NameAr = "علم الأورام",               DescriptionEn = "Cancer diagnosis and treatment",                       DescriptionAr = "تشخيص وعلاج السرطان" },
        new() { NameEn = "Hematology",                NameAr = "أمراض الدم",                DescriptionEn = "Blood disorders and diseases",                         DescriptionAr = "اضطرابات وأمراض الدم" },
        new() { NameEn = "Dentistry",                 NameAr = "طب الأسنان",                DescriptionEn = "Oral health and dental care",                          DescriptionAr = "صحة الفم والرعاية السنية" },
        new() { NameEn = "Plastic Surgery",           NameAr = "الجراحة التجميلية",         DescriptionEn = "Reconstructive and cosmetic surgery",                  DescriptionAr = "الجراحة الترميمية والتجميلية" },
        new() { NameEn = "General Surgery",           NameAr = "الجراحة العامة",            DescriptionEn = "Surgical procedures for various conditions",           DescriptionAr = "الإجراءات الجراحية لحالات متنوعة" },
        new() { NameEn = "Vascular Surgery",          NameAr = "جراحة الأوعية الدموية",     DescriptionEn = "Blood vessel diseases and surgery",                    DescriptionAr = "أمراض الأوعية الدموية وجراحتها" },
        new() { NameEn = "Neurosurgery",              NameAr = "جراحة المخ والأعصاب",       DescriptionEn = "Surgical treatment of nervous system disorders",        DescriptionAr = "العلاج الجراحي لاضطرابات الجهاز العصبي" },
        new() { NameEn = "Physical Therapy",          NameAr = "العلاج الطبيعي",            DescriptionEn = "Rehabilitation and physical rehabilitation",            DescriptionAr = "إعادة التأهيل والتأهيل الجسدي" },
        new() { NameEn = "Radiology",                 NameAr = "الأشعة التشخيصية",          DescriptionEn = "Medical imaging and diagnostic radiology",             DescriptionAr = "التصوير الطبي والأشعة التشخيصية" },
        new() { NameEn = "Anesthesiology",            NameAr = "التخدير",                   DescriptionEn = "Anesthesia and pain management",                       DescriptionAr = "التخدير وإدارة الألم" },
        new() { NameEn = "Emergency Medicine",        NameAr = "طب الطوارئ",                DescriptionEn = "Acute and emergency medical care",                     DescriptionAr = "الرعاية الطبية الحادة والطارئة" },
        new() { NameEn = "Sports Medicine",           NameAr = "طب الرياضة",                DescriptionEn = "Athletic injuries and sports-related conditions",       DescriptionAr = "إصابات الرياضيين والحالات المرتبطة بالرياضة" },
        new() { NameEn = "Geriatrics",                NameAr = "طب الشيخوخة",               DescriptionEn = "Healthcare for elderly patients",                      DescriptionAr = "الرعاية الصحية للمرضى المسنين" },
    ];
}
