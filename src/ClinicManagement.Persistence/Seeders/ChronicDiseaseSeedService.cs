using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

public class ChronicDiseaseSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChronicDiseaseSeedService> _logger;

    public ChronicDiseaseSeedService(ApplicationDbContext context, ILogger<ChronicDiseaseSeedService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedChronicDiseasesAsync()
    {
        if (await _context.Set<ChronicDisease>().AnyAsync()) return;

        _context.Set<ChronicDisease>().AddRange(Items);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} chronic diseases", Items.Length);
    }

    // ── Seed data (replaces chronic-diseases.json) ────────────────────────────

    private static readonly ChronicDisease[] Items =
    [
        new() { NameEn = "Type 1 Diabetes",                NameAr = "السكري من النوع الأول",           DescriptionEn = "Autoimmune condition where the pancreas produces little or no insulin",                DescriptionAr = "حالة مناعية ذاتية حيث ينتج البنكرياس القليل من الأنسولين أو لا ينتجه" },
        new() { NameEn = "Type 2 Diabetes",                NameAr = "السكري من النوع الثاني",          DescriptionEn = "Metabolic disorder affecting how the body processes blood sugar",                     DescriptionAr = "اضطراب أيضي يؤثر على كيفية معالجة الجسم لسكر الدم" },
        new() { NameEn = "Hypertension",                   NameAr = "ارتفاع ضغط الدم",                 DescriptionEn = "Persistently elevated blood pressure in the arteries",                               DescriptionAr = "ارتفاع مستمر في ضغط الدم في الشرايين" },
        new() { NameEn = "Coronary Artery Disease",        NameAr = "مرض الشريان التاجي",              DescriptionEn = "Narrowing of the coronary arteries due to plaque buildup",                           DescriptionAr = "تضيق الشرايين التاجية بسبب تراكم اللويحات" },
        new() { NameEn = "Heart Failure",                  NameAr = "قصور القلب",                      DescriptionEn = "Condition where the heart cannot pump enough blood",                                  DescriptionAr = "حالة لا يستطيع فيها القلب ضخ كمية كافية من الدم" },
        new() { NameEn = "Atrial Fibrillation",            NameAr = "الرجفان الأذيني",                  DescriptionEn = "Irregular and often rapid heart rate",                                               DescriptionAr = "معدل ضربات قلب غير منتظم وغالباً سريع" },
        new() { NameEn = "Asthma",                         NameAr = "الربو",                            DescriptionEn = "Chronic respiratory condition causing airway inflammation",                          DescriptionAr = "حالة تنفسية مزمنة تسبب التهاب مجرى الهواء" },
        new() { NameEn = "COPD",                           NameAr = "مرض الانسداد الرئوي المزمن",       DescriptionEn = "Chronic obstructive pulmonary disease affecting breathing",                          DescriptionAr = "مرض رئوي انسدادي مزمن يؤثر على التنفس" },
        new() { NameEn = "Chronic Kidney Disease",         NameAr = "مرض الكلى المزمن",                 DescriptionEn = "Gradual loss of kidney function over time",                                          DescriptionAr = "فقدان تدريجي لوظائف الكلى بمرور الوقت" },
        new() { NameEn = "Liver Cirrhosis",                NameAr = "تليف الكبد",                       DescriptionEn = "Scarring of the liver tissue due to long-term damage",                              DescriptionAr = "تندب نسيج الكبد بسبب الضرر طويل الأمد" },
        new() { NameEn = "Hypothyroidism",                 NameAr = "قصور الغدة الدرقية",               DescriptionEn = "Underactive thyroid gland producing insufficient hormones",                          DescriptionAr = "غدة درقية خاملة تنتج هرمونات غير كافية" },
        new() { NameEn = "Hyperthyroidism",                NameAr = "فرط نشاط الغدة الدرقية",           DescriptionEn = "Overactive thyroid gland producing excess hormones",                                 DescriptionAr = "غدة درقية مفرطة النشاط تنتج هرمونات زائدة" },
        new() { NameEn = "Rheumatoid Arthritis",           NameAr = "التهاب المفاصل الروماتويدي",        DescriptionEn = "Autoimmune disease causing joint inflammation and damage",                           DescriptionAr = "مرض مناعي ذاتي يسبب التهاب المفاصل وتلفها" },
        new() { NameEn = "Osteoarthritis",                 NameAr = "هشاشة العظام",                     DescriptionEn = "Degenerative joint disease causing cartilage breakdown",                            DescriptionAr = "مرض مفصلي تنكسي يسبب تكسر الغضروف" },
        new() { NameEn = "Osteoporosis",                   NameAr = "ترقق العظام",                      DescriptionEn = "Reduced bone density increasing fracture risk",                                      DescriptionAr = "انخفاض كثافة العظام مما يزيد من خطر الكسور" },
        new() { NameEn = "Epilepsy",                       NameAr = "الصرع",                            DescriptionEn = "Neurological disorder causing recurrent seizures",                                   DescriptionAr = "اضطراب عصبي يسبب نوبات متكررة" },
        new() { NameEn = "Multiple Sclerosis",             NameAr = "التصلب المتعدد",                   DescriptionEn = "Autoimmune disease affecting the central nervous system",                           DescriptionAr = "مرض مناعي ذاتي يؤثر على الجهاز العصبي المركزي" },
        new() { NameEn = "Parkinson's Disease",            NameAr = "مرض باركنسون",                     DescriptionEn = "Progressive nervous system disorder affecting movement",                            DescriptionAr = "اضطراب تدريجي في الجهاز العصبي يؤثر على الحركة" },
        new() { NameEn = "Alzheimer's Disease",            NameAr = "مرض الزهايمر",                     DescriptionEn = "Progressive brain disorder causing memory loss",                                    DescriptionAr = "اضطراب دماغي تدريجي يسبب فقدان الذاكرة" },
        new() { NameEn = "Depression",                     NameAr = "الاكتئاب",                         DescriptionEn = "Persistent mood disorder affecting daily functioning",                              DescriptionAr = "اضطراب مزاجي مستمر يؤثر على الأداء اليومي" },
        new() { NameEn = "Anxiety Disorder",               NameAr = "اضطراب القلق",                     DescriptionEn = "Chronic excessive worry and fear affecting daily life",                             DescriptionAr = "قلق وخوف مفرط مزمن يؤثر على الحياة اليومية" },
        new() { NameEn = "Crohn's Disease",                NameAr = "مرض كرون",                         DescriptionEn = "Inflammatory bowel disease affecting the digestive tract",                          DescriptionAr = "مرض التهابي في الأمعاء يؤثر على الجهاز الهضمي" },
        new() { NameEn = "Ulcerative Colitis",             NameAr = "التهاب القولون التقرحي",            DescriptionEn = "Inflammatory bowel disease causing colon ulcers",                                   DescriptionAr = "مرض التهابي في الأمعاء يسبب قرحة القولون" },
        new() { NameEn = "Celiac Disease",                 NameAr = "مرض الاضطرابات الهضمية",           DescriptionEn = "Autoimmune reaction to gluten affecting the small intestine",                       DescriptionAr = "تفاعل مناعي ذاتي للغلوتين يؤثر على الأمعاء الدقيقة" },
        new() { NameEn = "Lupus",                          NameAr = "الذئبة الحمراء",                   DescriptionEn = "Systemic autoimmune disease affecting multiple organs",                             DescriptionAr = "مرض مناعي ذاتي جهازي يؤثر على أعضاء متعددة" },
        new() { NameEn = "HIV/AIDS",                       NameAr = "فيروس نقص المناعة البشرية",         DescriptionEn = "Viral infection affecting the immune system",                                       DescriptionAr = "عدوى فيروسية تؤثر على الجهاز المناعي" },
        new() { NameEn = "Hepatitis B",                    NameAr = "التهاب الكبد الوبائي ب",            DescriptionEn = "Viral liver infection that can become chronic",                                     DescriptionAr = "عدوى فيروسية في الكبد يمكن أن تصبح مزمنة" },
        new() { NameEn = "Hepatitis C",                    NameAr = "التهاب الكبد الوبائي ج",            DescriptionEn = "Viral liver infection causing chronic liver disease",                               DescriptionAr = "عدوى فيروسية في الكبد تسبب مرض الكبد المزمن" },
        new() { NameEn = "Sickle Cell Disease",            NameAr = "مرض فقر الدم المنجلي",             DescriptionEn = "Inherited blood disorder affecting red blood cell shape",                           DescriptionAr = "اضطراب دموي وراثي يؤثر على شكل خلايا الدم الحمراء" },
        new() { NameEn = "Thalassemia",                    NameAr = "الثلاسيميا",                       DescriptionEn = "Inherited blood disorder causing abnormal hemoglobin",                              DescriptionAr = "اضطراب دموي وراثي يسبب هيموغلوبين غير طبيعي" },
        new() { NameEn = "Psoriasis",                      NameAr = "الصدفية",                          DescriptionEn = "Autoimmune skin condition causing rapid skin cell growth",                          DescriptionAr = "حالة جلدية مناعية ذاتية تسبب نمو سريع لخلايا الجلد" },
        new() { NameEn = "Eczema",                         NameAr = "الأكزيما",                         DescriptionEn = "Chronic skin condition causing inflammation and itching",                           DescriptionAr = "حالة جلدية مزمنة تسبب التهاباً وحكة" },
        new() { NameEn = "Gout",                           NameAr = "النقرس",                           DescriptionEn = "Form of arthritis caused by uric acid crystal buildup",                            DescriptionAr = "شكل من أشكال التهاب المفاصل ناجم عن تراكم بلورات حمض اليوريك" },
        new() { NameEn = "Migraine",                       NameAr = "الصداع النصفي",                    DescriptionEn = "Recurring severe headaches often with nausea and sensitivity",                     DescriptionAr = "صداع شديد متكرر غالباً مع غثيان وحساسية" },
        new() { NameEn = "Sleep Apnea",                    NameAr = "انقطاع التنفس أثناء النوم",         DescriptionEn = "Breathing repeatedly stops and starts during sleep",                               DescriptionAr = "توقف التنفس وبدؤه بشكل متكرر أثناء النوم" },
        new() { NameEn = "Obesity",                        NameAr = "السمنة",                           DescriptionEn = "Excess body fat increasing risk of other health conditions",                       DescriptionAr = "زيادة دهون الجسم مما يزيد من خطر الإصابة بحالات صحية أخرى" },
    ];
}
