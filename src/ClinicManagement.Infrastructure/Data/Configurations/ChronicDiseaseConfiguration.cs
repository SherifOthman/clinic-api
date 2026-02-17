using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations;

public class ChronicDiseaseConfiguration : IEntityTypeConfiguration<ChronicDisease>
{
    public void Configure(EntityTypeBuilder<ChronicDisease> builder)
    {
        builder.ToTable("ChronicDiseases");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.NameEn).HasMaxLength(80).IsRequired();
        builder.Property(e => e.NameAr).HasMaxLength(80).IsRequired();
        builder.Property(e => e.DescriptionEn).HasMaxLength(255);
        builder.Property(e => e.DescriptionAr).HasMaxLength(255);

        // Indexes
        builder.HasIndex(cd => cd.NameEn);
        builder.HasIndex(cd => cd.NameAr);

        // Seed data for common chronic diseases
        builder.HasData(
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd111111-1111-1111-1111-111111111111"), 
                NameEn = "Diabetes Type 1", 
                NameAr = "السكري النوع الأول",
                DescriptionEn = "A chronic condition in which the pancreas produces little or no insulin",
                DescriptionAr = "حالة مزمنة لا ينتج فيها البنكرياس الأنسولين أو ينتج القليل منه"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd222222-2222-2222-2222-222222222222"), 
                NameEn = "Diabetes Type 2", 
                NameAr = "السكري النوع الثاني",
                DescriptionEn = "A chronic condition that affects the way the body processes blood sugar",
                DescriptionAr = "حالة مزمنة تؤثر على طريقة معالجة الجسم لسكر الدم"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd333333-3333-3333-3333-333333333333"), 
                NameEn = "Hypertension", 
                NameAr = "ارتفاع ضغط الدم",
                DescriptionEn = "A condition in which the blood vessels have persistently raised pressure",
                DescriptionAr = "حالة ترتفع فيها ضغط الأوعية الدموية بشكل مستمر"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd444444-4444-4444-4444-444444444444"), 
                NameEn = "Asthma", 
                NameAr = "الربو",
                DescriptionEn = "A respiratory condition marked by attacks of spasm in the bronchi",
                DescriptionAr = "حالة تنفسية تتميز بنوبات تشنج في الشعب الهوائية"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd555555-5555-5555-5555-555555555555"), 
                NameEn = "Chronic Kidney Disease", 
                NameAr = "مرض الكلى المزمن",
                DescriptionEn = "A condition characterized by a gradual loss of kidney function over time",
                DescriptionAr = "حالة تتميز بفقدان تدريجي لوظائف الكلى مع مرور الوقت"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd666666-6666-6666-6666-666666666666"), 
                NameEn = "Heart Disease", 
                NameAr = "أمراض القلب",
                DescriptionEn = "A range of conditions that affect the heart",
                DescriptionAr = "مجموعة من الحالات التي تؤثر على القلب"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd777777-7777-7777-7777-777777777777"), 
                NameEn = "Arthritis", 
                NameAr = "التهاب المفاصل",
                DescriptionEn = "Inflammation of one or more joints, causing pain and stiffness",
                DescriptionAr = "التهاب في مفصل واحد أو أكثر، يسبب الألم والتصلب"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd888888-8888-8888-8888-888888888888"), 
                NameEn = "COPD", 
                NameAr = "مرض الانسداد الرئوي المزمن",
                DescriptionEn = "Chronic obstructive pulmonary disease - a group of lung conditions",
                DescriptionAr = "مرض الانسداد الرئوي المزمن - مجموعة من أمراض الرئة"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cd999999-9999-9999-9999-999999999999"), 
                NameEn = "Depression", 
                NameAr = "الاكتئاب",
                DescriptionEn = "A mental health disorder characterized by persistent sadness",
                DescriptionAr = "اضطراب في الصحة العقلية يتميز بالحزن المستمر"
            },
            new ChronicDisease 
            { 
                Id = Guid.Parse("cdaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 
                NameEn = "Obesity", 
                NameAr = "السمنة",
                DescriptionEn = "A medical condition involving excessive body fat",
                DescriptionAr = "حالة طبية تنطوي على دهون الجسم المفرطة"
            }
        );
    }
}
